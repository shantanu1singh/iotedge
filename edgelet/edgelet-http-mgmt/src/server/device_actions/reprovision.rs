// Copyright (c) Microsoft. All rights reserved.

//use std::sync::Arc;

use failure::ResultExt;
//use futures::Future;
use futures::{Future, IntoFuture};
use hyper::{Body, Request, Response, StatusCode};
use log::debug;

use edgelet_http::route::{Handler, Parameters};
use edgelet_http::Error as HttpError;
//use provisioning::provisioning::{Provision};
//use futures::sync::oneshot::{Sender};
use std::sync::mpsc::{Sender};

use crate::error::{Error, ErrorKind};
use crate::IntoResponse;
//use crate::IntoResponse;

//pub struct ReprovisionDevice<P> {
//    provision: P,
//}
//
//impl<P> ReprovisionDevice<P> {
//    pub fn new(provision: P) -> Self {
//        ReprovisionDevice { provision }
//    }
//}
//
//impl<P> Handler<Parameters> for ReprovisionDevice<P>
//where
//    P: 'static + Clone + Provision + Send + Sync,
//{
//    fn handle(
//        &self,
//        _req: Request<Body>,
//        _params: Parameters,
//    ) -> Box<dyn Future<Item = Response<Body>, Error = HttpError> + Send> {
//        debug!("Reprovision Device");
//        let provision = self.provision.clone();
//
//        let response = provision
//            .reprovision()
//            .then(|_| -> Result<_, Error> {
//                let response = Response::builder()
//                    .status(StatusCode::OK)
//                    .body(Body::default())
//                    .context(ErrorKind::ReprovisionDevice)?;
//                Ok(response)
//            })
//            .or_else(|e| Ok(e.into_response()));
//
//        Box::new(response)
//    }
//}

pub struct ReprovisionDevice {
//    initiate_shutdown: Arc<Sender<()>>,
    initiate_shutdown: Sender<()>,
}

impl ReprovisionDevice {
    pub fn new(initiate_shutdown: Sender<()>,) -> Self {
//        ReprovisionDevice { initiate_shutdown: Arc::new(initiate_shutdown) }
        ReprovisionDevice { initiate_shutdown }
    }
}

impl Handler<Parameters> for ReprovisionDevice
//    where
//        P: 'static + Clone + Provision + Send + Sync,
{
    fn handle(
        &self,
        _req: Request<Body>,
        _params: Parameters,
    ) -> Box<dyn Future<Item = Response<Body>, Error = HttpError> + Send> {
        debug!("Reprovision Device");
//        let provision = self.provision.clone();

        let response = self.initiate_shutdown.send(())
            .map_err(|_| Error::from(ErrorKind::ReprovisionDevice))
            .and_then(|_| -> Result<_, Error> {
                let response = Response::builder()
                    .status(StatusCode::OK)
                    .body(Body::default())
                    .context( ErrorKind::ReprovisionDevice)?;

                Ok(response)
            })
            .or_else(|e| Ok(e.into_response()))
//            .or_else(|e| Ok(Error::from(ErrorKind::ReprovisionDevice).into_response()))
            .into_future();

        Box::new(response)
    }
}

#[cfg(test)]
mod tests {
    use edgelet_core::{self, MakeModuleRuntime, ModuleRuntimeState};
    use edgelet_http::route::Parameters;
    use edgelet_test_utils::crypto::TestHsm;
    use edgelet_test_utils::module::*;
    use futures::Stream;
    use management::models::SystemInfo;

    use super::*;
    use crate::server::module::tests::Error;

    #[test]
    fn system_info_success() {
        // arrange
        let state = ModuleRuntimeState::default();
        let config = TestConfig::new("microsoft/test-image".to_string());
        let module: TestModule<Error, _> =
            TestModule::new("test-module".to_string(), config, Ok(state));
        let runtime = TestRuntime::make_runtime(
            TestSettings::new(),
            TestProvisioningResult::new(),
            TestHsm::default(),
        )
        .wait()
        .unwrap()
        .with_module(Ok(module));
        let handler = GetSystemInfo::new(runtime);
        let request = Request::get("http://localhost/info")
            .body(Body::default())
            .unwrap();

        // act
        let response = handler.handle(request, Parameters::new()).wait().unwrap();

        // assert
        response
            .into_body()
            .concat2()
            .and_then(|b| {
                let system_info: SystemInfo = serde_json::from_slice(&b).unwrap();
                let os_type = system_info.os_type();
                let architecture = system_info.architecture();

                assert_eq!("os_type_sample", os_type);
                assert_eq!("architecture_sample", architecture);
                assert_eq!(
                    edgelet_core::version_with_source_version(),
                    system_info.version(),
                );

                Ok(())
            })
            .wait()
            .unwrap();
    }

    #[test]
    fn system_info_failed() {
        // arrange
        let runtime = TestRuntime::make_runtime(
            TestSettings::new(),
            TestProvisioningResult::new(),
            TestHsm::default(),
        )
        .wait()
        .unwrap()
        .with_module(Err(Error::General));
        let handler = GetSystemInfo::new(runtime);
        let request = Request::get("http://localhost/modules")
            .body(Body::default())
            .unwrap();

        // act
        let response = handler.handle(request, Parameters::new()).wait().unwrap();

        // assert
        response
            .into_body()
            .concat2()
            .and_then(|b| {
                let error: ErrorResponse = serde_json::from_slice(&b).unwrap();
                assert_eq!(
                    "Could not query system info\n\tcaused by: General error",
                    error.message()
                );
                Ok(())
            })
            .wait()
            .unwrap();
    }
}
