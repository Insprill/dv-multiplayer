# Lobby Server - Rust

This is a [Rust](https://www.rust-lang.org/) implementation of the Derail Valley Lobby Server REST API service. The server can be run in either HTTP or HTTPS (SSL) modes (cert and key PEM files will need to be provided for SSL mode).

## Building the Code

To build the Lobby Server code, you'll need Rust, Cargo and OpenSSL installed on your system.


### Installing OpenSSL (Windows) 
OpenSSL can be installed as follows [[source](https://stackoverflow.com/a/61921362)]:
1. Download and extract the latest version of [vcpkg](https://github.com/microsoft/vcpkg/releases/)
2. Run `bootstrap-vcpkg.bat`
3. Run `vcpkg.exe install openssl-windows:x64-windows`
4. Run `vcpkg.exe install openssl:x64-windows-static`
5. Run `vcpkg.exe integrate install`
6. Run `set VCPKGRS_DYNAMIC=1`

### Building
The code can be built using `cargo build --release` or built and run (for testing purposes) using `cargo run --release`

## Configuration Parameters
The server can be configured using a `config.json` file; if one is not supplied, the server will create one with the defaults.

Below are the available parameters along with their defaults:
-   `port` (u16): The port number on which the server will listen. Default: `8080`
-   `timeout` (u64): The time-out period in seconds for server removal. Default: `120`
-   `ssl_enabled` (bool): Whether SSL is enabled. Default: `false`
-   `ssl_cert_path` (string): Path to the SSL certificate file. Default: `"cert.pem"`
-   `ssl_key_path` (string): Path to the SSL private key file. Default: `"key.pem"`

To customize these parameters, create a `config.json` file in the project directory with the desired values. Here's an example `config.json`:
```json
{
  "port": 8080,
  "timeout": 120,
  "ssl_enabled": false,
  "ssl_cert_path": "cert.pem",
  "ssl_key_path": "key.pem"
}
```

