// Copyright (c) Microsoft. All rights reserved.

use serde_derive::{Deserialize, Serialize};

/// This is the name of the network created by the iotedged
const DEFAULT_NETWORKID: &str = "azure-iot-edge";

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct Network {
    name: String,

    #[serde(rename = "ipv6", skip_serializing_if = "Option::is_none")]
    ipv6: Option<bool>,

    #[serde(rename = "ipam", skip_serializing_if = "Option::is_none")]
    ipam: Option<Vec<Ipam>>,
}

impl Network {
    pub fn name(&self) -> &str {
        &self.name
    }

    pub fn with_name(mut self, name: String) -> Self {
        self.name = name;
        self
    }

    pub fn ipv6(&self) -> Option<&bool> {
        self.ipv6.as_ref()
    }

    pub fn with_ipv6(mut self, ipv6: Option<bool>) -> Self {
        self.ipv6 = ipv6;
        self
    }

    pub fn ipam(&self) -> Option<&Vec<Ipam>> {
        self.ipam.as_ref().map(AsRef::as_ref)
    }

    pub fn with_ipam(mut self, ipam: Option<Vec<Ipam>>) -> Self {
        self.ipam = ipam;
        self
    }
}

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct Ipam {
    #[serde(rename = "gateway", skip_serializing_if = "Option::is_none")]
    gateway: Option<String>,

    #[serde(rename = "subnet", skip_serializing_if = "Option::is_none")]
    subnet: Option<String>,

    #[serde(rename = "ip_range", skip_serializing_if = "Option::is_none")]
    ip_range: Option<String>,
}

impl Ipam {
    pub fn gateway(&self) -> Option<&str> {
        self.gateway.as_ref().map(AsRef::as_ref)
    }

    pub fn with_gateway(mut self, gateway: Option<String>) -> Self {
        self.gateway = gateway;
        self
    }

    pub fn subnet(&self) -> Option<&str> {
        self.subnet.as_ref().map(AsRef::as_ref)
    }

    pub fn with_subnet(mut self, subnet: Option<String>) -> Self {
        self.subnet = subnet;
        self
    }

    pub fn ip_range(&self) -> Option<&str> {
        self.ip_range.as_ref().map(AsRef::as_ref)
    }

    pub fn with_ip_range(mut self, ip_range: Option<String>) -> Self {
        self.ip_range = ip_range;
        self
    }
}

#[derive(Clone, Debug, Deserialize, Serialize)]
#[serde(untagged)]
pub enum MobyNetwork {
    Network(Network),
    Name(String),
}

impl MobyNetwork {
    pub fn name(&self) -> &str {
        match self {
            MobyNetwork::Name(name) => {
                if name.is_empty() {
                    &DEFAULT_NETWORKID
                } else {
                    name
                }
            },
            MobyNetwork::Network(network) => &network.name
        }
    }
}