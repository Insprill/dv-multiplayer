<?php
include 'config.php';

// Create database connection
try {
    // Create a PDO instance
    $pdo = new PDO("mysql:host={$dbConfig['host']};dbname={$dbConfig['dbname']}", $dbConfig['username'], $dbConfig['password']);
    
    // Set PDO error mode to exception
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    // Now you can use $pdo to execute queries
} catch (PDOException $e) {
    // Handle database connection errors
    echo "Connection failed: " . $e->getMessage();
}


// Define routes
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	
	$data = json_decode(file_get_contents('php://input'), true);
	
    switch ($_SERVER['REQUEST_URI']) {
        case '/add_game_server':
            echo add_game_server($pdo, $data);
            break;
			
        case '/update_game_server':
            echo update_game_server($pdo, $data);
            break;
			
        case '/remove_game_server':
            echo remove_game_server($pdo, $data);
            break;
			
        default:
            http_response_code(404);
            break;
    }
	
} elseif ($_SERVER['REQUEST_METHOD'] === 'GET') {
    if ($_SERVER['REQUEST_URI'] === '/list_game_servers') {
        echo list_game_servers($pdo);
    } else {
        http_response_code(404);
    }
} else {
    http_response_code(405); // Method Not Allowed
}


function add_game_server($pdo, $data) {
    // Validation
    if (!validate_server_info($data)) {
        return json_encode(["error" => "Invalid server information"]);
    }

    // Generate a UUID for the game server
    $game_server_id = uuid_create();

    // Insert server information into the database
    $stmt = $pdo->prepare("INSERT INTO game_servers (game_server_id, ip, port, server_name, password_protected, game_mode, difficulty, time_passed, current_players, max_players, required_mods, game_version, multiplayer_version, server_info, last_update) 
                            VALUES (:game_server_id, :ip, :port, :server_name, :password_protected, :game_mode, :difficulty, :time_passed, :current_players, :max_players, :required_mods, :game_version, :multiplayer_version, :server_info, :last_update)");
    $stmt->execute([
        ':game_server_id' => $game_server_id,
        ':ip' => $data['ip'],
        ':port' => $data['port'],
        ':server_name' => $data['server_name'],
        ':password_protected' => $data['password_protected'],
        ':game_mode' => $data['game_mode'],
        ':difficulty' => $data['difficulty'],
        ':time_passed' => $data['time_passed'],
        ':current_players' => $data['current_players'],
        ':max_players' => $data['max_players'],
        ':required_mods' => $data['required_mods'],
        ':game_version' => $data['game_version'],
        ':multiplayer_version' => $data['multiplayer_version'],
        ':server_info' => $data['server_info'],
        ':last_update' => time() // Assuming Unix timestamp for last_update
    ]);

    // Return game server ID
    return json_encode(["game_server_id" => $game_server_id]);
}


function update_game_server($pdo, $data) {
    // Update current players count and time passed for the specified game server
    $stmt = $pdo->prepare("UPDATE game_servers 
                            SET current_players = :current_players, time_passed = :time_passed, last_update = :last_update
                            WHERE game_server_id = :game_server_id");
    $stmt->execute([
        ':current_players' => $data['current_players'],
        ':time_passed' => $data['time_passed'],
        ':last_update' => time(), // Assuming Unix timestamp for last_update
        ':game_server_id' => $data['game_server_id']
    ]);

    // Check if update was successful
    if ($stmt->rowCount() > 0) {
        return json_encode(["message" => "Server updated"]);
    } else {
        return json_encode(["error" => "Failed to update server"]);
    }
}


function remove_game_server($pdo, $data) {
    // Delete the specified game server from the database
    $stmt = $pdo->prepare("DELETE FROM game_servers WHERE game_server_id = :game_server_id");
    $stmt->execute([':game_server_id' => $data['game_server_id']]);

    // Check if deletion was successful
    if ($stmt->rowCount() > 0) {
        return json_encode(["message" => "Server removed"]);
    } else {
        return json_encode(["error" => "Failed to remove server"]);
    }
}


function list_game_servers($pdo) {
    // Retrieve the list of game servers from the database
    $stmt = $pdo->query("SELECT * FROM game_servers");
    $servers = $stmt->fetchAll(PDO::FETCH_ASSOC);

    // Return the list of game servers
    return json_encode($servers);
}


/* 
	**************************************
   
			Helper functions
   
	*************************************
*/
   

function validate_server_info($data) {
    // Check if server name length exceeds 25 characters
    if (strlen($data['server_name']) > 25) {
        return false;
    }

    // Check if server info length exceeds 500 characters
    if (strlen($data['server_info']) > 500) {
        return false;
    }

    // Check if current players exceed max players
    if ($data['current_players'] > $data['max_players']) {
        return false;
    }

    // Check if max players is at least 1
    if ($data['max_players'] < 1) {
        return false;
    }

    // If all checks pass, return true
    return true;
}


// Function to generate UUID
function uuid_create() {
    return sprintf('%04x%04x-%04x-%04x-%04x-%04x%04x%04x',
        mt_rand(0, 0xffff), mt_rand(0, 0xffff), mt_rand(0, 0xffff),
        mt_rand(0, 0x0fff) | 0x4000,
        mt_rand(0, 0x3fff) | 0x8000,
        mt_rand(0, 0xffff), mt_rand(0, 0xffff), mt_rand(0, 0xffff)
    );
}