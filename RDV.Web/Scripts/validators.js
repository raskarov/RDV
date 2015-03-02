/**
 * Глобальный объект, содержащий все валидаторы форм
 * @constructor - конструктор по умолчанию
 */
function Validators() {

    /**
     * Заполняем стандартные сообщения валидатора
     * @type {String}
     */
    $.validator.messages.required = "Обязательное поле";
    $.validator.messages.email = "Неверный формат Email";
    $.validator.messages.digits = "Допустимы только цифры";
    $.validator.messages.equalTo = "Значения не совпадают";
    $.validator.messages.minlength = "Минимальная длина {0} символов";
    $.validator.messages.maxlength = "Максимальная длина {0} символов";
    $.validator.messages.date = "Неверный формат даты";

    /**
     * Добавляем метод валидации русских дат
     * @type {*}
     */
    jQuery.validator.addMethod(
            "dateRU",
            function (value, element) {
                var check = false;
                var re = /^\d{1,2}\.\d{1,2}\.\d{4}$/;
                if (re.test(value)) {
                    var adata = value.split('.');
                    var mm = parseInt(adata[1], 10);
                    var dd = parseInt(adata[0], 10);
                    var yyyy = parseInt(adata[2], 10);
                    var xdata = new Date(yyyy, mm - 1, dd);
                    if (( xdata.getFullYear() == yyyy ) && ( xdata.getMonth() == mm - 1 ) && ( xdata.getDate() == dd ))
                        check = true;
                    else
                        check = false;
                } else
                    check = false;
                return this.optional(element) || check;
            },
            "Неверный формат даты"
    );

    /**
     * Добавляем метод валидации для номера дома
     * @type {*}
     */
    jQuery.validator.addMethod(
            "houseNumber",
            function (value, element) {
                var check = false;
                var re = /^[а-я0-9]{1,}$/;
                if (re.test(value)) {
                    check = true;
                } else
                    check = false;
                return this.optional(element) || check;
            },
            "Неверный формат номера дома"
    );

    /**
     * Добавляет метод валидации чисел с плавающей запятой
     */
    jQuery.validator.addMethod(
        "floatNumbers",
        function (value,element){
            var re = /^[0-9]*\,?[0-9]*$/;
            var check = re.test(value);
            return this.optional(element) || check;
        },
        "Неверный формат числа с плавающей запятой"
    );

    /**
     * Запоминаем себя
     * @type {Validators}
     */
    var self = this;

    /**
     * Бандит валидаторы, специфичные для страницы регистрации нового пользователя
     */
    this.bindRegistrationFormValidators = function () {
        $("#reg-form").validate({
            rules:{
                Email:{
                    required:true,
                    email:true,
                    remote: '/account/check-email'
                },
                Password:{
                    required:true,
                    minlength:6
                },
                PasswordConfirm:{
                    required:true,
                    equalTo:"#regPasswordField"
                },
                FirstName:{
                    required:true,
                    maxlength:255
                },
                SurName:{
                    required:false,
                    maxlength:255
                },
                LastName:{
                    required:true,
                    maxlength:255
                },
                Phone:{
                    required:true,
                    maxlength:255
                },
                Phone2:{
                    required:false,
                    maxlength:255
                },
                Birthdate:{
                    required:false,
                    dateRU:true
                },
                ICQ:{
                    required:false,
                    digits:true
                },
                Appointment:{
                    required:false
                }
            },
            submitHandler:function (form) {
                var agreed = $("#acceptAgreementField").attr("checked") == "checked";
                if (!agreed) {
                    alert("Вы должны принять правила использования системы");
                    return;
                }
                form.submit();
            }
        });
    };

    /**
     * Байндит валидаторы формы редактирования профиля
     */
    this.bindProfileFormValidators = function () {
        $("#profile-form").validate({
            rules:{
                Email:{
                    required:true,
                    email:true
                },
                OldPassword:{
                    minlength:6
                },
                NewPassword:{
                    required:{
                        depends:function (element) {
                            var val = $("#profile-oldpassword-field").val();
                            return val != null && val != "";
                        },
                        message:"Введите новый пароль"
                    },
                    minlength:6

                },
                NewPasswordConfirm:{
                    equalTo:"#profile-newpassword-field"
                },
                FirstName:{
                    required:true,
                    maxlength:255
                },
                SurName:{
                    required:false,
                    maxlength:255
                },
                LastName:{
                    required:true,
                    maxlength:255
                },
                Phone:{
                    required:true,
                    maxlength:255
                },
                Phone2:{
                    required:false,
                    maxlength:255
                },
                Birthdate:{
                    required:false,
                    dateRU:true
                },
                ICQ:{
                    required:false,
                    digits:true
                },
                Appointment:{
                    required:false
                },
                SeniorityStartDate:{
                    required:false,
                    dateRU:true
                },
                CertificateNumber:{
                    required:false
                },
                CertificationDate:{
                    required:false,
                    dateRU:true
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму учебной программы
     */
    this.bindTrainingProgramFormValidatorsAndSubmitHandler = function (submitCallback) {
        $("#training-program-form").validate({
            rules:{
                TrainingDate:{
                    required:true,
                    dateRU:true
                },
                ProgramName:{
                    required:true
                },
                Organizer:{
                    required:true
                },
                TrainingPlace:{
                    required:true
                }
            }
        });
    };

    /**
     * Байндит валидатор на форму создания новой роли
     * @param submitCallback
     */
    this.bindNewRoleFormValidators = function (submitCallback) {
        $("#new-role-form").validate({
            rules:{
                Name:{
                    required:true
                }
            },
            submitHandler:function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидатор на форму переименования роли
     * @param submitCallback
     */
    this.bindRenameRoleFormValidators = function (submitCallback) {
        $("#rename-role-form").validate({
            rules:{
                Name:{
                    required:true
                }
            },
            submitHandler:function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидатор на форму редактирования пользователя
     * @param submitCallback
     */
    this.bindUserFormValidators = function () {
        $("#edit-user-form").validate({
            rules:{
                Email:{
                    required:true,
                    email:true,
                },
                Password:{
                    required:{
                        depends:function () {
                            return $("user-is-edit-field").val() == 0;
                        }
                    },
                    minlength:6
                },
                PasswordConfirm:{
                    required:{
                        depends:function () {
                            return $("user-is-edit-field").val() == 0;
                        }
                    },
                    equalTo:"#user-password-field"
                },
                FirstName:{
                    required:true,
                    maxlength:255
                },
                SurName:{
                    required:false,
                    maxlength:255
                },
                LastName:{
                    required:true,
                    maxlength:255
                },
                Phone:{
                    required:true,
                    maxlength:255
                },
                Phone2:{
                    required:false,
                    maxlength:255
                },
                Birthdate:{
                    required:false,
                    dateRU:true
                },
                ICQ:{
                    required:false,
                    digits:true
                },
                Appointment:{
                    required:false
                },
                CertificateNumber:{
                    required:false,
                    maxlength: 255
                },
                CertificationDate:{
                    required:false,
                    dateRU: true
                },
                CertificateEndDate:{
                    required:false,
                    dateRU: true
                },
                SeniorityStartDate:{
                    required:false,
                    dateRU: true
                },
                PublicLoading:{
                    required:false,
                    maxlength: 4000
                },
                AdditionalInformation:{
                    required:false,
                    maxlength: 4000
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму создания/редактирования компании
     * @param submitCallback
     */
    this.bindCompanyFormValidators = function (submitCallback) {
        $("#edit-company-form").validate({
            rules:{
                Name:{
                    required:true,
                    maxlength:255
                },
                Description:{
                    maxlength:4000
                },
                Email:{
                    required:true,
                    email:true
                },
                Phone1:{
                    required:true
                },
                Address:{
                    maxlength:4000
                },
                Branches:{
                    maxlength:4000
                },
                ContactPerson:{
                    required:true,
                    maxlength:255
                }
            },
            submitHandler:function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидатор на форму создания справочников
     * @param submitCallback
     */
    this.bindNewDictionaryFormValidators = function (submitCallback) {
        $("#new-dictionary-form").validate({
            rules:{
                SystemName:{
                    required:true
                },
                DisplayName:{
                    required:true
                }
            },
            submitHandler:function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидатор на форму переименования справочника
     * @param submitCallback
     */
    this.bindRenameDictionaryFormValidators = function (submitCallback) {
        $("#rename-dictionary-form").validate({
            rules:{
                SystemName:{
                    required:true
                },
                DisplayName:{
                    required:true
                }
            },
            submitHandler:function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидатор на форму переименования справочника
     * @param submitCallback
     */
    this.bindEditDictionaryValueFormValidators = function (submitCallback) {
        $("#edit-dictionary-value-form").validate({
            rules:{
                Value:{
                    required:true,
                    maxlength:4000
                }
            },
            submitHandler:function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму редактирования профиля своей компании
     */
    this.bindEditCompanyProfileValidators = function() {
        $("#profile-form").validate({
            rules: {
                Name: {
                    required: true,
                    maxlength: 255,
                    minlength: 5
                },
                Description: {
                    required: true,
                    maxlength: 1500
                },
                Address: {
                    required: true,
                    maxlength: 4000
                },
                Phone1: {
                    required: true,
                    maxlength: 255
                },
                Phone2: {
                    maxlength: 255
                },
                Phone3: {
                    maxlength: 255
                },
                Email: {
                    required: true,
                    email: true
                },
                Branch: {
                    maxlength: 255
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму создания или редактирования страницы
     */
    this.bindEditStaticPageFormValidators = function() {
        $("#edit-page-form").validate({
            rules: {
                Title: {
                    required: true,
                    maxlength: 255
                },
                Route: {
                    required: true,
                    maxlength: 255
                },
                Content: {
                    required: true
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму создания или редактирования публикации
     */
    this.bindEditNewsFormValidators = function() {
        $("#edit-news-form").validate({
            rules: {
                Title: {
                    required: true,
                    maxlength: 255
                },
                ShortContent: {
                    required: true,
                    maxlength: 255
                },
                FullContent: {
                    required: true
                },
                VideoLink: {
                    maxlength: 4000
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму создания или редактирования публикации
     */
    this.bindEditMenuItemFormValidators = function() {
        $("#menu-item-form").validate({
            rules: {
                Title: {
                    required: true,
                    maxlength: 255
                },
                Href: {
                    required: true,
                    maxlength: 4000
                },
                Position: {
                    number: true
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму создания или редактирования клиента
     */
    this.bindClientFormValidators = function() {
        $("#edit-client-form").validate({
            rules: {
                FirstName: {
                    required: true,
                    maxlength: 255
                },
                LastName: {
                    required: false,
                    maxlength: 255
                },
                SurName: {
                    required: false,
                    maxlength: 255
                },
                Phone: {
                    maxlength: 255,
                    required: true
                },
                ICQ: {
                    maxlength: 255,
                    digits: false
                },
                Address: {
                    maxlength: 4000
                },
                Notes: {
                    maxlength: 4000
                },
                AgreementNumber: {
                    maxlength: 255
                },
                Comission: {
                    maxlength: 255,
                    digits: true
                },
                Email: {
                    required: false,
                    email: true
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму создания или редактирования клиента
     */
    this.bindClientAjaxFormValidators = function(submitCallback) {
        $("#new-client-form").validate({
            rules: {
                FirstName: {
                    required: true,
                    maxlength: 255
                },
                LastName: {
                    required: false,
                    maxlength: 255
                },
                SurName: {
                    required: false,
                    maxlength: 255
                },
                Phone: {
                    maxlength: 255,
                    required: true
                },
                ICQ: {
                    maxlength: 255,
                    digits: true
                },
                Address: {
                    maxlength: 4000
                },
                Birthdate: {
                    dateRU: true
                },
                Email: {
                    required: false,
                    email: true
                }
            },
            submitHandler: function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };

    /**
     * Байндит валидаторы на форму изменения статуса объектов
     */
    this.bindChangeObjectStatusFormValidators = function() {
        $("#change-object-status-form").validate({
            rules: {
                delayDate: {
                    dateRU: true,
                    required: function(element){
                        return $("#objects-status-field").val() == "3";
                    }
                },
                counterAgentCompany: {
                    maxlength: 255,
                    required: function(element){
                        return ($("#objects-status-field").val() == "2" || $("#objects-status-field").val() == "5") && ($("#counter-agent-type-field").val() == "2");
                    }
                },
                advanceDate: {
                    dateRU: true,
                    required: function(element){
                        return $("#objects-status-field").val() == "2";
                    }
                },
                dealDate: {
                    dateRU: true,
                    required: function(element){
                        return $("#objects-status-field").val() == "5";
                    }
                },
                realPrice: {
                    floatNumbers: true,
                    required: function(element){
                        return $("#objects-status-field").val() == "2" || $("#objects-status-field").val() == "5";
                    }
                }
            },
            submitHandler: function(form){
                var objStatus = $("#objects-status-field").val();
                var caType = $("#counter-agent-type-field").val();
                var clientId = $("#counter-agent-client-id-field").val();
                if ((objStatus == "2" || objStatus == "5") && caType == "3" && clientId == "-1"){
                    alert("Вы должны выбрать клиента из списка либо создать нового клиента");
                } else {
                    form.submit();
                }

            }
        });
    };

	/**
	 * Байндит валидаторы на формы обратной связи
	 * @param submitHandler
	 */
	this.bindFeedbackFormValidators = function(submitHandler){
		$("#feedback-form").validate({
			rules: {
				Name: {
					required: true,
					maxlength: 255
				},
				Email: {
					required: true,
					maxlength: 255,
					email: true
				},
				Phone: {
					maxlength: 255
				},
				Message: {
					required: true,
					maxlength: 4000
				}
			},
			submitHandler: function(form){
				if (submitHandler != undefined){
					submitHandler();
				}
			}
		});
	};

	/**
	 * Байндит валидаторы на форму сообщения об ошибке
	 * @param submitHandler
	 */
	this.bindBugReportFormValidators = function(submitHandler){
		$("#bug-report-form").validate({
			rules: {
				ReporterName: {
					required: true,
					maxlength: 255
				},
				ReporterEmail: {
					required: true,
					maxlength: 255,
					email: true
				},
				ReportMessage: {
					required: true,
					maxlength: 4000
				}
			},
			submitHandler: function(form){
				if (submitHandler != undefined){
					submitHandler();
				}
			}
		});
    };
    
    /**
	 * Байндит валидаторы на форму создания рассылки
	 * @param submitHandler
	 */
    this.bindEditNewsLetterFormValidators = function() {
        $("#edit-newsletter-form").validate({
			rules: {
				Subject: {
					required: true,
					maxlength: 255
				},
				Content: {
					required: true
				}
			}
		});
    };

    /**
    * Байндит валидаторы на форму добавления достижения
    */
    this.bindAchievmentFormValidators = function() {
        $("#training-achievment-form").validate({
            rules: {
                Title: {
                    required: true,
                    maxlength: 255
                },
                ReachDate: {
                    required: true,
                    dateRU: true
                },
                Organizer: {
                    required: true
                }
            }
        });
    };

    /**
    * Байндит валидаторы на форму добавления достижения
    */
    this.bindClientReviewFormValidators = function () {
        $("#client-review-form").validate({
            rules: {
                ReviewDate: {
                    required: true,
                    dateRU: true
                }
            }
        });
    };
    
    /**
    * Байндит валидаторы на форму изменения цены объекта
    */
    this.bindChangeObjectPriceValidators = function() {
        $("#change-price-form").validate({
            rules: {
                ownerPrice: {
                    required: true,
                    floatNumbers: true
                },
                price: {
                    required: true,
                    floatNumbers: true
                }
            }
        });
    };
    
    /**
    * Байндит валидаторы на форму редактирования услуги
    */
    this.bindEditServiceFormValidators = function() {
        $("#edit-service-form").validate({
            rules: {
                ServiceName: {
                    required: true,
                    maxlength: 255
                },
                Tax: {
                    floatNumbers: true,
                    required: true
                },
                RDVShare: {
                    floatNumbers: true,
                    required: true,
                },
                ContractDate: {
                    dateRU: true
                }
            },
            submitHandler: function (form) {
                var providerId = $("#provider-id-field").val();
                if (providerId > 0) {
                    form.submit();
                } else {
                    alert("Введите поставищка услуг");
                }
            }
        });
    };
    
    /**
     * Байндит валидаторы на форму редактирования книжки
     **/
    this.bindEditBookFormValidators = function() {
        $("#edit-book-form").validate({
            rules: {
                Title: {
                    required: true,
                    maxlength: 255
                },
                Author: {
                    required: true,
                    maxlength: 255
                },
                Publisher: {
                    required: true,
                    maxlength: 255
                },
                Price: {
                    required: true,
                    floatNumbers: true
                }
            }             
        });
    };
    
    /**
     * Байндит валидаторы на форму заказа книжки
     **/
    this.bindOrderBookFormValidators = function() {
        $("#order-book-form").validate({
            rules: {
                title: {
                    required: true,
                },
                fio: {
                    required: true,
                },
                email: {
                    required: true,
                    email: true
                },
                phone: {
                    required: true
                }
            }
        });
    };
    
    /**
     * Байндит валидаторы на форму редактирования партнера
     **/
    this.bindEditPartnerFormValidators = function() {
        $("#edit-partner-form").validate({
           rules: {
               Name: {
                   required: true,
                   maxlength: 255
               },
               Url : {
                   required: true,
                   maxlength: 255
               },
               Position: {
                   required: true,
                   digits: true
               }
           }             
        });
    };
    
    /**
      * Отображает форму заказа аренды класса
      */
    this.bindOrderClassFormValidators = function() {
        $("#order-class-form").validate({
           rules: {
               fio: {
                   required: true,
               },
               email: {
                   required: true,
                   email: true
               },
               phone: {
                   required: true
               }
           }             
        });
    };
    
    /**
      * Отображает валидаторы на форму создания нового клиента
      */
    this.bindNonRdvAjaxFormValidators = function(submitCallback) {
        $("#new-non-rdv-agent-form").validate({
            rules: {
                FirstName: {
                    required: true,
                    maxlength: 255
                },
                LastName: {
                    required: false,
                    maxlength: 255
                },
                SurName: {
                    required: false,
                    maxlength: 255
                },
                Phone: {
                    maxlength: 255,
                    required: true
                }
            },
            submitHandler: function (form) {
                if (submitCallback != undefined) {
                    submitCallback(form);
                }
            }
        });
    };
}
var validators = new Validators();