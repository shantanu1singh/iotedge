/*
 * IoT Edge Management API
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2018-06-28
 *
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

use serde_derive::{Deserialize, Serialize};
#[allow(unused_imports)]
use serde_json::Value;

#[derive(Clone, Debug, Serialize, Deserialize)]
pub struct DeviceConnectionInfo {
    #[serde(rename = "hubId")]
    hub_id: String,
    #[serde(rename = "deviceId")]
    device_id: String,
}

impl DeviceConnectionInfo {
    pub fn new(hub_id: String, device_id: String) -> Self {
        DeviceConnectionInfo { hub_id, device_id }
    }

    pub fn set_hub_id(&mut self, hub_id: String) {
        self.hub_id = hub_id;
    }

    pub fn with_hub_id(mut self, hub_id: String) -> Self {
        self.hub_id = hub_id;
        self
    }

    pub fn hub_id(&self) -> &String {
        &self.hub_id
    }

    pub fn set_device_id(&mut self, device_id: String) {
        self.device_id = device_id;
    }

    pub fn with_device_id(mut self, device_id: String) -> Self {
        self.device_id = device_id;
        self
    }

    pub fn device_id(&self) -> &String {
        &self.device_id
    }
}
