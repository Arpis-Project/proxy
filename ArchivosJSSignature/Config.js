/**
 * ButtonHook corre antes de Print/Update de Recepciones
 * Procesa de forma especial los Recepcion Devolucion
 */
ButtonHooksManager.addHandler(['before_recvUpdateOnly', 'before_recvPrintUpdate'],
    function ($q, $uibModal, SharedData, alertService, ModelService, $http, FuncionesService,NotificationService,$translate) {
        var deferred = $q.defer();
        var DocType = '';
        var secWaitBeforCloseMessage = 2000;
        var sharedData = SharedData;
		if (SharedData.receiving.voucher.info.voutype == 1) //ES UN VOUCHER RETURN
        {
			    NotificationService.ajaxLoader($translate.instant('E-113'), $translate.instant('E-114'));	
				try{
					var reqUploadSettings = {
						   url: "/v1/rest/callDTEVyVVoucher",
						   method: "POST",
						   dataType: "json",
						   data: { "documentID": SharedData.receiving.voucher.info.sid },
						   headers: { "accept": "text/json; charset=utf-8;", "auth-session": sessionStorage.getItem("PRISMAUTH") }
					};
					$http(reqUploadSettings).then(function (responseUpload) {
						console.log(responseUpload);
						console.log(responseUpload.status);
						if (responseUpload.status == 200) {
							var respuesta = JSON.parse(responseUpload.data);
							console.log(respuesta);
							if (respuesta.Estatus == "0") {
							   var FolioNo = respuesta.FolioNo;
							   var TED = atob(respuesta.TED);
								SharedData.receiving.voucher.info.trackingno = FolioNo;
								$(".modal").remove();
								$(".modal-backdrop").remove();
								deferred.resolve();

							}
							else {
							   console.log("error");
							   var Error1 = '';
							   var Error2 = '';

							   try {
								   Error1 = 'Documento no autorizado, c\u00F3digo de error ' + respuesta.Estatus;
								   Error2 = respuesta.MsgEstatus ;
							   } catch (error) { }

							   var modalOptions = {
								   closeButtonText: 'Cerrar',
								   headerText: 'Error',
								   bodyText1: Error1,
								   bodyText2: Error2
							   };
							   $(".modal").remove();
							   $(".modal-backdrop").remove();
							   setTimeout(function () { waitingDialog.hide(); }, 0);
							   alertService.showModal({}, modalOptions)
								   .then(function (result) {
									   deferred.reject();
								   });
							}

						}
						else if (responseUpload.status == 501)
						{
						   setTimeout(function () { waitingDialog.hide(); }, 0);

						   var modalOptions555 = {
							   closeButtonText: 'Cerrar',
							   headerText: 'Error',
							   bodyText1: 'Error: No hay comunicacion con la aplicacion Proxy. Por favor reinicie el Proxy.',
							   bodyText2: ''
						   };
						   setTimeout(function () { waitingDialog.hide(); }, 0);
						   $(".modal").remove();
							$(".modal-backdrop").remove();
						   alertService.showModal({}, modalOptions555)
							   .then(function (result) {
								   deferred.reject();
							   });
					    }
						else//estatuscode != 200
						{
						   setTimeout(function () { waitingDialog.hide(); }, 0);

						   var modalOptions555 = {
							   closeButtonText: 'Cerrar',
							   headerText: 'Error',
							   bodyText1: 'Error: No hay comunicacion con la aplicacion Proxy. Por favor reinicie el Proxy.',
							   bodyText2: ''
						   };
						   setTimeout(function () { waitingDialog.hide(); }, 0);
						   $(".modal").remove();
							$(".modal-backdrop").remove();
						   alertService.showModal({}, modalOptions555)
							   .then(function (result) {
								   deferred.reject();
							   });

						}

					}, function (errorLogin) {
					   console.log("ERROR");
					   console.log(errorLogin);
					   if (errorLogin.status == "555")
					   {
						   setTimeout(function () { waitingDialog.hide(); }, 0);

						   var modalOptions555 = {
							   closeButtonText: 'Cerrar',
							   headerText: 'Error',
							   bodyText1: 'Error: ' + errorLogin.data,
							   bodyText2: ''
						   };
						   setTimeout(function () { waitingDialog.hide(); }, 0);
							$(".modal").remove();
							$(".modal-backdrop").remove();
							alertService.showModal({}, modalOptions555)
							   .then(function (result) {
								   deferred.reject();
								});
					   }
					   else if (errorLogin.status == "501")
					   {
						   setTimeout(function () { waitingDialog.hide(); }, 0);

						   var modalOptions555 = {
							   closeButtonText: 'Cerrar',
							   headerText: 'Error',
							   bodyText1: 'Error: No hay comunicacion con la aplicacion Proxy. Por favor reinicie el Proxy.',
							   bodyText2: ''
						   };
						   setTimeout(function () { waitingDialog.hide(); }, 0);
						   $(".modal").remove();
							$(".modal-backdrop").remove();
						   alertService.showModal({}, modalOptions555)
							   .then(function (result) {
								   deferred.reject();
							   });
					   }
					   else {
						   setTimeout(function () { waitingDialog.hide(); }, 0);
						   var modalOptions555 = {
							   closeButtonText: 'Cerrar',
							   headerText: 'Error',
							   bodyText1: 'Error: ' + errorLogin.statusTex,
							   bodyText2: ''
						   };
						   setTimeout(function () { waitingDialog.hide(); }, 0);
						   $(".modal").remove();
							$(".modal-backdrop").remove();
						   alertService.showModal({}, modalOptions555)
							   .then(function (result) {
								   deferred.reject();
							   });
					   }
					});
				} catch (error) { 
					setTimeout(function () { waitingDialog.hide(); }, 0);
						   var modalOptions555 = {
							   closeButtonText: 'Cerrar',
							   headerText: 'Error',
							   bodyText1: 'No es posible comunicarse con el Proxy',
							   bodyText2: ''
						   };
						   setTimeout(function () { waitingDialog.hide(); }, 0);
						   $(".modal").remove();
							$(".modal-backdrop").remove();
						   alertService.showModal({}, modalOptions555)
							   .then(function (result) {
								   deferred.reject();
							   });
				}	
			
		}else{
			deferred.resolve();
		}	
		

        return deferred.promise;
    }

);

/**
 * ButtonHook corre antes de Print/Update de SLIP Transferencias
 */
ButtonHooksManager.addHandler(['before_slipUpdateOnly', 'before_slipPrintUpdate'],
    function ($q, $uibModal, TransferSlip, alertService, ModelService2, ModelService, $http, FuncionesService,NotificationService,$translate) {
	    var deferredSlip = $q.defer();
        var DocType = '';
        var secWaitBeforCloseMessage = 2000;
        var transferSlip = TransferSlip;
		var continuar = true;
		ModelService2.get('Transferslip', { sid: transferSlip.slip.sid, cols: 'trackingno' })
        .then(function (slipT) {
                var slipTrackingNo = slipT[0].trackingno;
                if (slipTrackingNo != null) {
                    if (slipTrackingNo.trim() != '')
                        continuar = false;
                }
                if (continuar == false) {
                    waitingDialog.show('Documento ya tiene un Nro de Folio Asignado. ' + slipTrackingNo);
                    setTimeout(function () { waitingDialog.hide(); }, secWaitBeforCloseMessage);
                    deferredSlip.resolve();
                }
                else {
                    NotificationService.ajaxLoader($translate.instant('E-113'), $translate.instant('E-114'));		
				
					try {
						var reqUploadSettings = {
							   url: "/v1/rest/callDTEVyVTransfer",
							   method: "POST",
							   dataType: "json",
							   data: { "documentID": transferSlip.slip.sid},
							   headers: { "accept": "text/json; charset=utf-8;", "auth-session": sessionStorage.getItem("PRISMAUTH") }
						};
						$http(reqUploadSettings).then(function (responseUpload) {
							console.log(responseUpload);
							if (responseUpload.status == 200) {
								var respuesta = JSON.parse(responseUpload.data);
								console.log(respuesta);
								if (respuesta.Estatus == "0") {
								   var FolioNo = respuesta.FolioNo;
								   var TED = atob(respuesta.TED);
								   
								   //HACER UPDATE EN PRISM
									ModelService2.get('Transferslip', { sid: transferSlip.slip.sid, cols: '*' })
									.then(function (slipDoc) {
											var slip = slipDoc[0];
											slip.trackingno = FolioNo;
											slip.save();

											ModelService2.get('Transferslip', { sid: transferSlip.slip.sid, cols: '*' })
												.then(function (slipDoc) {
													$(".modal").remove();
													$(".modal-backdrop").remove();
													deferredSlip.resolve();
												});

									},
									function () {
										deferredSlip.reject('Error actualizando la información del documento');
									});
								}
								else {
								   console.log("error");
								   var Error1 = '';
								   var Error2 = '';

								   try {
									   Error1 = 'Documento no autorizado, c\u00F3digo de error ' + respuesta.Estatus;
									   Error2 = respuesta.MsgEstatus ;
								   } catch (error) { }

								   var modalOptions = {
									   closeButtonText: 'Cerrar',
									   headerText: 'Error',
									   bodyText1: Error1,
									   bodyText2: Error2
								   };
								   $(".modal").remove();
								   $(".modal-backdrop").remove();
								   setTimeout(function () { waitingDialog.hide(); }, 0);
								   alertService.showModal({}, modalOptions)
									   .then(function (result) {
										   deferred.reject();
									   });
								}

							}
							else//estatuscode != 200
							{
								if (responseUpload.status == 501)
								{
									setTimeout(function () { waitingDialog.hide(); }, 0);

									var modalOptions555 = {
									   closeButtonText: 'Cerrar',
									   headerText: 'Error',
									   bodyText1: 'Error: No hay comunicacion con la aplicacion Proxy. Por favor reinicie el Proxy.',
									   bodyText2: ''
									};
									setTimeout(function () { waitingDialog.hide(); }, 0);
									$(".modal").remove();
									$(".modal-backdrop").remove();
									alertService.showModal({}, modalOptions555)
									.then(function (result) {
										deferred.reject();
									}); 
								}
								else
								{
								   var modalOptions = {
									   closeButtonText: 'Cerrar',
									   headerText: 'Error',
									   bodyText1: 'Documento no autorizado: ' + dataUpload.split(String.fromCharCode(165))[1].split(':')[1],
									   bodyText2: ''
								   };
								   console.log("4");
								   setTimeout(function () { waitingDialog.hide(); }, 0);
								   $(".modal").remove();
								   $(".modal-backdrop").remove();
								   alertService.showModal({}, modalOptions)
									   .then(function (result) {
										   deferred.reject();
									   });
								}
							}

						}, function (errorLogin) {
						   console.log("ERROR");
						   console.log(errorLogin);
						   if (errorLogin.status == "555")
						   {
							   setTimeout(function () { waitingDialog.hide(); }, 0);

							   var modalOptions555 = {
								   closeButtonText: 'Cerrar',
								   headerText: 'Error',
								   bodyText1: 'Error: ' + errorLogin.data,
								   bodyText2: ''
							   };
							   setTimeout(function () { waitingDialog.hide(); }, 0);
							   $(".modal").remove();
								$(".modal-backdrop").remove();
							   alertService.showModal({}, modalOptions555)
								   .then(function (result) {
									   deferred.reject();
								   });
						   }
						   else if (errorLogin.status == "501")
						   {
							   setTimeout(function () { waitingDialog.hide(); }, 0);

							   var modalOptions555 = {
								   closeButtonText: 'Cerrar',
								   headerText: 'Error',
								   bodyText1: 'Error: No hay comunicacion con la aplicacion Proxy. Por favor reinicie el Proxy.',
								   bodyText2: ''
							   };
							   setTimeout(function () { waitingDialog.hide(); }, 0);
								$(".modal").remove();
								$(".modal-backdrop").remove();
								alertService.showModal({}, modalOptions555)
								   .then(function (result) {
									   deferred.reject();
								   });
						   }
						   else {
							   setTimeout(function () { waitingDialog.hide(); }, 0);
							   var modalOptions555 = {
								   closeButtonText: 'Cerrar',
								   headerText: 'Error',
								   bodyText1: 'Error: ' + errorLogin.statusTex,
								   bodyText2: ''
							   };
							   setTimeout(function () { waitingDialog.hide(); }, 0);
							   $(".modal").remove();
								$(".modal-backdrop").remove();
							   alertService.showModal({}, modalOptions555)
								   .then(function (result) {
									   deferred.reject();
								   });
						   }
						});
					} catch (error) { 
						setTimeout(function () { waitingDialog.hide(); }, 0);
							   var modalOptions555 = {
								   closeButtonText: 'Cerrar',
								   headerText: 'Error',
								   bodyText1: 'No es posible comunicarse con el Proxy',
								   bodyText2: ''
							   };
							   setTimeout(function () { waitingDialog.hide(); }, 0);
							   $(".modal").remove();
								$(".modal-backdrop").remove();
							   alertService.showModal({}, modalOptions555)
								   .then(function (result) {
									   deferred.reject();
								   });
					}
	
                }
            });

        return deferredSlip.promise;
    }

);