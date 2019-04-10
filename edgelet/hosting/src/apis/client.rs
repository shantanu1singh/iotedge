use std::rc::Rc;

use super::configuration::Configuration;
use hyper;

pub struct APIClient<C: hyper::client::Connect> {
    configuration: Rc<Configuration<C>>,
    hosting_api: Box<::apis::HostingApi>,
}

impl<C: hyper::client::Connect> APIClient<C> {
    pub fn new(configuration: Configuration<C>) -> APIClient<C> {
        let rc = Rc::new(configuration);

        APIClient {
            configuration: rc.clone(),
            hosting_api: Box::new(::apis::HostingApiClient::new(rc.clone())),
        }
    }

    pub fn hosting_api(&self) -> &::apis::HostingApi {
        self.hosting_api.as_ref()
    }
}
