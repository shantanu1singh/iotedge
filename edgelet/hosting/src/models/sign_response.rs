/*
 * IoT Edge Hosting Environment API
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2019-04-10
 *
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

use serde_derive::{Deserialize, Serialize};
#[allow(unused_imports)]
use serde_json::Value;

#[derive(Debug, Serialize, Deserialize)]
pub struct SignResponse {
    /// Signature of the data.
    #[serde(rename = "digest")]
    digest: String,
}

impl SignResponse {
    pub fn new(digest: String) -> SignResponse {
        SignResponse { digest: digest }
    }

    pub fn set_digest(&mut self, digest: String) {
        self.digest = digest;
    }

    pub fn with_digest(mut self, digest: String) -> SignResponse {
        self.digest = digest;
        self
    }

    pub fn digest(&self) -> &String {
        &self.digest
    }
}
