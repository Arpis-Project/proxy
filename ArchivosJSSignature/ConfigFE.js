/**
 * ButtonHook corre antes de Print/Update de Document
 * Procesa de forma especial los documentReturn
 */
ButtonHooksManager.addHandler(['before_navPosTenderUpdateOnly','before_navPosTenderPrintUpdate'],
	function($q, $uibModal,$log, ExternalDataService,alertService,ModelService,$http,FuncionesService,modalNCReqInfService,NotificationService,$translate) {
		var deferred = $q.defer();
		scope = ExternalDataService.scope; 
		var secWaitBeforCloseMessage=2000;

		if(scope.document.pos_flag1=="35 BOLETA")
		{
			var workstation_name;
			ModelService.get('Proxy').then(function(datawk){

			console.log(datawk[0]);

			workstation_name= datawk[0]['workstation_name'];
			console.log('FC-WorkstationName: '+workstation_name);
	
			ModelService.get('Proxy',{filter:'workstation_name,eq,'+ workstation_name,cols:'sid'}).then(function(data){
				console.log(data[0]);

				ModelService.get('ProxyExtension',{proxy_sid:data[0].sid,cols:'*'})
				.then(function( datas){
					console.log(datas[0]);

					var lisconfiguration = JSON.parse(datas[0]['configuration']);
					BoletaManualNo= lisconfiguration.BM; 
					console.log('FC-BoletaManualNo: '+BoletaManualNo);
					if(BoletaManualNo==undefined)
					{
						deferred.reject('Error--El documento debe tener un número de folio');

					}
					else
					{   
					   updateInvoice(scope, $q, BoletaManualNo, "", "", "", "")
					   .then(function (resultUpdate) {
						   if (resultUpdate === true) {

							var  proxBoletaManualNo= (parseInt(BoletaManualNo)+1);
							lisconfiguration.BM= proxBoletaManualNo;
							datas[0]['configuration']= lisconfiguration;
							datas[0].save();
							deferred.resolve(); //PROCESA LA VENTA DE FORMA NORMAL
						   }
						   else {
							   var Error1 = '';
							   var Error2 = '';

							   try {
								   Error1 = 'Fallo la actualizacion del Folio';
								   Error2 = '';
							   } catch (error) { }

							   var modalOptions = {
								   closeButtonText: 'Cerrar',
								   headerText: 'Error',
								   bodyText1: Error1,
								   bodyText2: Error2
							   };
							   setTimeout(function () { waitingDialog.hide(); }, 0);
							   alertService.showModal({}, modalOptions)
								   .then(function (result) {
									   deferred.reject();
								   });

						   }


					   })
					   .catch(function (err) {
						   setTimeout(function () { waitingDialog.hide(); }, secWaitBeforCloseMessage);
						   alert(err);
						   deferred.reject();
					   });
					}

					});
			});

		});

		}else{			
				NotificationService.ajaxLoader($translate.instant('E-113'), $translate.instant('E-114'));
				try{
					var reqUploadSettings = {
							   url: "/v1/rest/callDTEVyV",
							   method: "POST",
							   dataType: "json",
							   data: { "documentID": scope.document.sid },
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

							updateInvoice(scope, $q, FolioNo, "", "", "", TED).then(function (resultUpdate) {
								sessionStorage.setItem("VYV_UltDoc",scope.document.sid );
								if (resultUpdate === true) {
									$(".modal").remove();
									$(".modal-backdrop").remove();
								   deferred.resolve();

								}else {
								   var Error1 = '';
								   var Error2 = '';

								   try {
									   Error1 = 'Fallo la actualizacion del Folio';
									   Error2 = '';
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


							   })
							   .catch(function (err) {
								   $(".modal").remove();
								   $(".modal-backdrop").remove();
								   setTimeout(function () { waitingDialog.hide(); }, secWaitBeforCloseMessage);
								   alert(err);
								   deferred.reject();
							   });
						}
						else {
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
			}catch (error) { 
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
  
		return deferred.promise;
		
		
	function updateInvoice(scope,$q, Folio, Comment1, Comment2, status, TED){
       var deferred = $q.defer();
       var promise = deferred.promise;
		
       ModelService.get('Document', { sid: scope.document.sid })
       .then(function(invoiceDoc){
           scope.document = invoiceDoc[0];
           if (Comment2!='')
               scope.document.comment2 = Comment2;
           if (Comment1!='')
               scope.document.comment1 = Comment1;
           if (Folio != '') {
                scope.document.fiscal_document_number = Folio;
				scope.document.tracking_number = Folio;
				scope.document.document_number = Folio;
           }
           if (status!="")
               scope.document.status=status;
           if (TED !="")
               scope.document.udf_clob1 = TED;
			  scope.document.save().then(function(data){
					deferred.resolve(true);
			  });
       },function(){deferred.reject('Error actualizando la informaci\u00F3n del documento');});

       return promise;
	}
	
	}
);