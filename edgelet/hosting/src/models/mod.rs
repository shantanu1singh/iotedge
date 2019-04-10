mod device_connection_info;
pub use self::device_connection_info::DeviceConnectionInfo;
mod error_response;
pub use self::error_response::ErrorResponse;
mod sign_request;
pub use self::sign_request::SignRequest;
mod sign_response;
pub use self::sign_response::SignResponse;

// TODO(farcaller): sort out files
pub struct File;
