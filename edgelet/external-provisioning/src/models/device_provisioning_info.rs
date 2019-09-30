/*
 * IoT Edge External Provisioning Environment API
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2019-04-10
 *
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

#[allow(unused_imports)]
use serde_json::Value;

#[derive(Clone, Debug, serde_derive::Serialize, serde_derive::Deserialize)]
pub struct DeviceProvisioningInfo {
    /// The host name of the IoT hub.
    #[serde(rename = "hubName")]
    hub_name: String,
    /// The ID of the device in IoT hub.
    #[serde(rename = "deviceId")]
    device_id: String,
    #[serde(rename = "credentials")]
    credentials: crate::models::Credentials,
    /// The registration/provisioning status of the device in IoT hub.
    #[serde(rename = "status", skip_serializing_if="Option::is_none")]
    status: Option<String>,
    /// The registration/provisioning sub-status of 'assigned' devices in IoT hub.
    #[serde(rename = "substatus", skip_serializing_if="Option::is_none")]
    substatus: Option<String>,
}

impl DeviceProvisioningInfo {
    pub fn new(
        hub_name: String,
        device_id: String,
        credentials: crate::models::Credentials,
    ) -> DeviceProvisioningInfo {
        DeviceProvisioningInfo {
            hub_name,
            device_id,
            credentials,
            status: None,
            substatus: None
        }
    }

    pub fn set_hub_name(&mut self, hub_name: String) {
        self.hub_name = hub_name;
    }

    pub fn with_hub_name(mut self, hub_name: String) -> DeviceProvisioningInfo {
        self.hub_name = hub_name;
        self
    }

    pub fn hub_name(&self) -> &str {
        &self.hub_name
    }

    pub fn set_device_id(&mut self, device_id: String) {
        self.device_id = device_id;
    }

    pub fn with_device_id(mut self, device_id: String) -> DeviceProvisioningInfo {
        self.device_id = device_id;
        self
    }

    pub fn device_id(&self) -> &str {
        &self.device_id
    }

    pub fn set_credentials(&mut self, credentials: crate::models::Credentials) {
        self.credentials = credentials;
    }

    pub fn with_credentials(
        mut self,
        credentials: crate::models::Credentials,
    ) -> DeviceProvisioningInfo {
        self.credentials = credentials;
        self
    }

    pub fn credentials(&self) -> &crate::models::Credentials {
        &self.credentials
    }

    pub fn set_status(&mut self, status: String) {
        self.status = Some(status);
    }

    pub fn with_status(mut self, status: String) -> DeviceProvisioningInfo {
        self.status = Some(status);
        self
    }

    pub fn status(&self) -> Option<&str> {
        self.status.as_ref().map(AsRef::as_ref)
    }

    pub fn reset_status(&mut self) {
        self.status = None;
    }

    pub fn set_substatus(&mut self, substatus: String) {
        self.substatus = Some(substatus);
    }

    pub fn with_substatus(mut self, substatus: String) -> DeviceProvisioningInfo {
        self.substatus = Some(substatus);
        self
    }

    pub fn substatus(&self) -> Option<&str> {
        self.substatus.as_ref().map(AsRef::as_ref)
    }

    pub fn reset_substatus(&mut self) {
        self.substatus = None;
    }
}
