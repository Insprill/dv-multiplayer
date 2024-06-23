use actix_web::{web, App, HttpResponse, HttpServer, Responder};
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::fs::File;
use std::io::{Read, Write};
use std::sync::{Arc, Mutex};
use std::time::{SystemTime, UNIX_EPOCH};
use env_logger::Env;
use log::{info, error};
use uuid::Uuid;
use openssl::ssl::{SslAcceptor, SslFiletype, SslMethod};
use openssl::ssl::SslAcceptorBuilder;

#[derive(Serialize, Deserialize, Clone)]
struct ServerInfo {
    ip: String,
    port: u16,
    server_name: String,
    password_protected: bool,
    game_mode: u8,
    difficulty: u8,
    time_passed: String,
    current_players: u32,
    max_players: u32,
    required_mods: String,
    game_version: String,
    multiplayer_version: String,
    server_info: String,
    #[serde(skip_serializing)]
    last_update: u64,
}

#[derive(Serialize, Deserialize, Clone)]
struct AddServerResponse {
    game_server_id: String,
}

#[derive(Clone)]
struct AppState {
    servers: Arc<Mutex<HashMap<String, ServerInfo>>>,
}

#[derive(Serialize, Deserialize, Clone)]
struct Config {
    port: u16,
    timeout: u64,
    ssl_enabled: bool,
    ssl_cert_path: String,
    ssl_key_path: String,
}

impl Default for Config {
    fn default() -> Self {
        Config {
            port: 8080,
            timeout: 120,
            ssl_enabled: false,
            ssl_cert_path: String::from("cert.pem"),
            ssl_key_path: String::from("key.pem"),
        }
    }
}

fn read_or_create_config() -> Config {
    let config_path = "config.json";
    let mut config = Config::default();

    if let Ok(mut file) = File::open(config_path) {
        let mut contents = String::new();
        if file.read_to_string(&mut contents).is_ok() {
            if let Ok(parsed_config) = serde_json::from_str(&contents) {
                config = parsed_config;
            }
        }
    } else {
        if let Ok(mut file) = File::create(config_path) {
            let _ = file.write_all(serde_json::to_string_pretty(&config).unwrap().as_bytes());
        }
    }

    config
}

#[tokio::main]
async fn main() -> std::io::Result<()> {
    env_logger::Builder::from_env(Env::default().default_filter_or("info")).init();

    let config = read_or_create_config();
    let state = AppState {
        servers: Arc::new(Mutex::new(HashMap::new())),
    };

    let server = {
        let server_builder = HttpServer::new(move || {
            App::new()
                .app_data(web::Data::new(state.clone()))
                .route("/add_game_server", web::post().to(add_server))
                .route("/update_game_server", web::post().to(update_server))
                .route("/remove_game_server", web::post().to(remove_server))
                .route("/list_game_servers", web::get().to(list_servers))
        });
    
        if config.ssl_enabled {
            let ssl_builder = setup_ssl(&config)?;
            server_builder.bind_openssl(format!("0.0.0.0:{}", config.port), (move || ssl_builder)())?
        } else {
            server_builder.bind(format!("0.0.0.0:{}", config.port))?
        }
    };
    

    // Start the server
    server.run().await
}

fn setup_ssl(config: &Config) -> std::io::Result<SslAcceptorBuilder> {
    let mut builder = SslAcceptor::mozilla_intermediate(SslMethod::tls())?;
    builder.set_private_key_file(&config.ssl_key_path, SslFiletype::PEM)?;
    builder.set_certificate_chain_file(&config.ssl_cert_path)?;
    Ok(builder)
}

fn validate_server_info(info: &ServerInfo) -> Result<(), &'static str> {
    if info.server_name.len() > 25 {
        return Err("Server name exceeds 25 characters");
    }
    if info.server_info.len() > 500 {
        return Err("Server info exceeds 500 characters");
    }
    if info.current_players > info.max_players {
        return Err("Current players exceed max players");
    }
    if info.max_players < 1 {
        return Err("Max players must be at least 1");
    }
    Ok(())
}

#[derive(Deserialize)]
struct AddServerRequest {
    ip: String,
    port: u16,
    server_name: String,
    password_protected: bool,
    game_mode: u8,
    difficulty: u8,
    time_passed: String,
    current_players: u32,
    max_players: u32,
    required_mods: String,
    game_version: String,
    multiplayer_version: String,
    server_info: String,
}

async fn add_server(data: web::Data<AppState>, server_info: web::Json<AddServerRequest>) -> impl Responder {
    let info = ServerInfo {
        ip: server_info.ip.clone(),
        port: server_info.port,
        server_name: server_info.server_name.clone(),
        password_protected: server_info.password_protected,
        game_mode: server_info.game_mode,
        difficulty: server_info.difficulty,
        time_passed: server_info.time_passed.clone(),
        current_players: server_info.current_players,
        max_players: server_info.max_players,
        required_mods: server_info.required_mods.clone(),
        game_version: server_info.game_version.clone(),
        multiplayer_version: server_info.multiplayer_version.clone(),
        server_info: server_info.server_info.clone(),
        last_update: SystemTime::now().duration_since(UNIX_EPOCH).unwrap().as_secs(),
    };

    if let Err(e) = validate_server_info(&info) {
        error!("Validation failed: {}", e);
        return HttpResponse::BadRequest().json(e);
    }
    
    let game_server_id = Uuid::new_v4().to_string();
    let key = game_server_id.clone();
    match data.servers.lock() {
        Ok(mut servers) => {
            servers.insert(key.clone(), info);
            info!("Server added: {}", key);
            HttpResponse::Ok().json(AddServerResponse { game_server_id: key })
        }
        Err(_) => {
            error!("Failed to add server: {}", key);
            HttpResponse::InternalServerError().json("Failed to add server")
        }
    }
}

#[derive(Deserialize)]
struct UpdateServerRequest {
    game_server_id: String,
    current_players: u32,
    time_passed: String,
}

async fn update_server(data: web::Data<AppState>, server_info: web::Json<UpdateServerRequest>) -> impl Responder {
    let mut updated = false;
    match data.servers.lock() {
        Ok(mut servers) => {
            if let Some(info) = servers.get_mut(&server_info.game_server_id) {
                if server_info.current_players <= info.max_players {
                    info.current_players = server_info.current_players;
                    info.time_passed = server_info.time_passed.clone();
                    info.last_update = SystemTime::now().duration_since(UNIX_EPOCH).unwrap().as_secs();
                    updated = true;
                }
            }
        }
        Err(_) => {
            error!("Failed to update server: {}", server_info.game_server_id);
            return HttpResponse::InternalServerError().json("Failed to update server");
        }
    }
    
    if updated {
        info!("Server updated: {}", server_info.game_server_id);
        HttpResponse::Ok().json("Server updated")
    } else {
        error!("Server not found or invalid current players: {}", server_info.game_server_id);
        HttpResponse::BadRequest().json("Server not found or invalid current players")
    }
}

#[derive(Deserialize)]
struct RemoveServerRequest {
    game_server_id: String,
}

async fn remove_server(data: web::Data<AppState>, server_info: web::Json<RemoveServerRequest>) -> impl Responder {
    let removed = match data.servers.lock() {
        Ok(mut servers) => servers.remove(&server_info.game_server_id).is_some(),
        Err(_) => {
            error!("Failed to remove server: {}", server_info.game_server_id);
            false
        }
    };

    if removed {
        info!("Server removed: {}", server_info.game_server_id);
        HttpResponse::Ok().json("Server removed")
    } else {
        error!("Server not found: {}", server_info.game_server_id);
        HttpResponse::BadRequest().json("Server not found")
    }
}

async fn list_servers(data: web::Data<AppState>) -> impl Responder {
    match data.servers.lock() {
        Ok(servers) => {
            let servers_list: Vec<ServerInfo> = servers.values().cloned().collect();
            HttpResponse::Ok().json(servers_list)
        }
        Err(_) => {
            error!("Failed to retrieve servers");
            HttpResponse::InternalServerError().json("Failed to retrieve servers")
        }
    }
}
