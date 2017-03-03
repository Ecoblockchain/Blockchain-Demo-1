/// <reference path="jquery-2.1.0-vsdoc.js" />
var WebApp = {
    Init: function () {
        WebApp.Plugins.Init();
        WebApp.Events.Init();
    },
    Data: {
        DataTable: null,
    },
    Plugins: {
        Init: function () {
            WebApp.Plugins.PrepareDataTables();
            WebApp.Plugins.ActivateMiscOnDemandPlugins();
            WebApp.Plugins.PrepareDateRange();
        },
        PrepareDataTables: function () {
            $('.data-table-local').each(function (index, table) {
                WebApp.Data.DataTable = $(table).DataTable({
                    dom: '<"datatable-header"fl><"datatable-scroll"t><"datatable-footer"ip>',
                    "oLanguage": {
                        "sLengthMenu": '<span class="hide">Show:</span> _MENU_',
                        "sSearch": '<span class="hide">Search:</span> _INPUT_',
                        "oPaginate": {
                            "sNext": '&rarr;',
                            "sLast": 'Last',
                            "sFirst": 'First',
                            "sPrevious": '&larr;'
                        }
                    },
                    responsive: false,
                    "bProcessing": true,
                    "bServerSide": false,
                    "bFilter": true,
                    "scrollX": true,
                    "bPaginate": true,
                    "aaSorting": [
                        [0, $(this).attr('data-type') == 'transactions' ? 'desc' : 'asc']
                    ],
                    "aoColumnDefs": [{
                        "bSortable": false,
                        "aTargets": ["no-sort"]
                    }],
                    "aLengthMenu": [
                        [10, 25, 50, 100],
                        [10, 25, 50, 100]
                    ],
                    "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                        switch ($(table).attr('data-type')) {
                            case 'statusPopover':
                                break;
                            default:
                                break;
                        }
                    },
                    fnDrawCallback: function (oSettings) {

                    }
                });
            })


            $('.data-table').each(function (index, table) {
                WebApp.Data.DataTable = $(table).DataTable({
                    dom: '<"datatable-header"fl><"datatable-scroll"t><"datatable-footer"ip>',
                    "oLanguage": {
                        "sLengthMenu": '<span class="hide">Show:</span> _MENU_',
                        "sSearch": '<span class="hide">Search:</span> _INPUT_',
                        "oPaginate": {
                            "sNext": '&rarr;',
                            "sLast": 'Last',
                            "sFirst": 'First',
                            "sPrevious": '&larr;'
                        }
                    },
                    responsive: true,
                    "bProcessing": true,
                    "bServerSide": true,
                    "bFilter": true,
                    "scrollX": true,
                    "bPaginate": true,
                    "aaSorting": [
                        [$(table).attr('data-key'), 'asc']
                    ],
                    "aoColumnDefs": [{
                        "bSortable": false,
                        "aTargets": ["no-sort"]
                    }],
                    "aLengthMenu": [
                        [10, 25, 50, 100],
                        [10, 25, 50, 100]
                    ],
                    "sAjaxSource": function () {
                        return $(table).attr('data-source')
                    }(),
                    "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                        switch ($(table).attr('data-type')) {
                            case 'statusPopover':
                                break;
                            default:
                                break;
                        }
                    },
                    fnDrawCallback: function (oSettings) {

                    }
                });
            });

            //$("input#recipient").livequery(function () {
            //    var input = document.getElementById('recipient');
            //    var awesomplete = new Awesomplete(input, {
            //        minChars: 1,
            //        filter: function (text, input) {
            //            return true;
            //        },
            //        //data: function (item, input) {
            //        //    return {
            //        //        label: item.text,
            //        //        value: item.id
            //        //    };
            //        //}
            //    });

            //    $(input).on("keyup", function () {
            //        $.ajax({
            //            url: '/requests/AutoComplete/?q=' + this.value,
            //            type: 'POST',
            //            dataType: 'json'
            //        }).success(function (data) {
            //            var list = [];
            //            $.each(data, function (key, value) {
            //                list.push({
            //                    label: value.text,
            //                    value: value.text
            //                });
            //            });
            //            awesomplete.list = list;
            //        });
            //    })
            //});

            $("#recipient").typeahead({
                hint: true,
                highlight: true,
                minLength: 1,
                source: function (request, response) {
                    $.ajax({
                        url: '/requests/AutoComplete/',
                        data: "{ 'q': '" + request + "'}",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            items = [];
                            map = {};
                            $.each(data, function (i, item) {
                                var _id = item.id;
                                var _text = item.text;
                                map[_text] = { id: _id, text: _text };
                                items.push(_text);
                            });
                            response(items);
                            $(".dropdown-menu").css("height", "auto");
                        },
                        error: function (response) {
                            alert(response.responseText);
                        },
                        failure: function (response) {
                            alert(response.responseText);
                        }
                    });
                },
                updater: function (item) {
                    $('#receiver').val(map[item].id);
                    return item;
                },
                matcher: function (item) {
                    return true;
                }
            });

            $("#To").typeahead({
                hint: true,
                highlight: true,
                minLength: 1,
                source: function (request, response) {
                    $.ajax({
                        url: '/wallet/AutoComplete/',
                        data: "{ 'q': '" + request + "'}",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            items = [];
                            map = {};
                            $.each(data, function (i, item) {
                                var _id = item.id;
                                var _text = item.text;
                                map[_text] = { id: _id, text: _text };
                                items.push(_text);
                            });
                            response(items);
                            $(".dropdown-menu").css("height", "auto");
                        },
                        error: function (response) {
                            alert(response.responseText);
                        },
                        failure: function (response) {
                            alert(response.responseText);
                        }
                    });
                },
                updater: function (item) {

                    $('#receiver').val(map[item].id);
                    return item;
                },
                matcher: function (item) {
                    return true;
                }
            });
        },

        PrepareDateRange: function () {
            $('.date-range').each(function (index, input) {
                $(input).daterangepicker({
                    ranges: {
                        'Today': [moment(), moment()],
                        'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                        'Last 7 Days': [moment().subtract('days', 6), moment()],
                        'Last 30 Days': [moment().subtract('days', 29), moment()],
                        'This Month': [moment().startOf('month'), moment().endOf('month')],
                        'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
                    },
                    startDate: moment().subtract('days', 29),
                    endDate: moment()

                }, function (start, end) {
                    $(input).val(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
                });

                $(input).parent().find('.input-group-btn').click(function () {
                    $(input).trigger('click')
                })
            });
        },
        ActivateMiscOnDemandPlugins: function () {
            $('.toggle-switch').livequery(function () {
                $(this).bootstrapToggle({
                    on: 'Yes',
                    off: 'No'
                });
            });

        }
    },
    Events: {
        Init: function () {
            WebApp.Events.BindStaticEvents();
            WebApp.Events.BindLiveEvents();
        },
        BindStaticEvents: function () {
            $(document)
                .on('click', '.confirm', function () {
                    return confirm($(this).attr('data-confirm'));
                })
              .on('change', '#Quantity', function () {
                  var asset = $(".Asset :selected").text();
                  var amount = asset.split('(')[1].split(' ')[0];
                  //alert(amount);
                  var _enteredamount = $("#Quantity").val()
                  //alert(_enteredamount)
                  var _status = "";
                  var _status = $("#status");

                  if (parseFloat(_enteredamount) > parseFloat(amount)) {
                      $("#Amount").val('');
                      _status.html("The amount you've entered is larger than your available balance.").addClass("text-danger");
                  }
                  else {
                      _status.css("display", "none");

                  }
              })

             .on('change', '.Criteria', function () {

                 if ($("#Criteria").val() != "") {
                     $(".CriteriaValueDiv").removeClass("hidden");
                     $("#CriteriaValue").attr("data-val", "true");
                     $("#CriteriaValue").attr("data-val-required", "Please enter a value for Criteria Value");

                     //$('.form-group').livequery(function () {
                     //    if ($(this).find('input[data-val-required],textarea[data-val-required],textarea[data-val-summernote]').not('[type="checkbox"]').length < 1) {
                     //        alert("df")
                     //        $(this).find('label').first().append('<span class="text-danger">&nbsp;*</span>');
                     //    }
                     //});
                 }
                 else {
                     $(".CriteriaValueDiv").addClass("hidden");
                     $("#CriteriaValue").removeAttr("data-val");
                     $("#CriteriaValue").removeAttr("data-val-required");
                 }
                 WebApp.Core.RebindFormValidation();
             })


        },
        BindLiveEvents: function () {
            $('select').not('.dataTables_length select').livequery(function () {
                $(this).select2();
                $(this).trigger('change');
            });

            $("html[dir='rtl']").livequery(function () {
                $(".navbar-brand img").attr("src", "/assets/images/logo-small.png");
                $(".paginate_button.previous, .paginate_button.next").addClass("rotate180");
            });

            $('select.auto-select').livequery(function () {
                $(this).val($(this).attr('data-selected'));
            });

            $('.data-table-local').livequery(function () {
                $('.dataTables_filter input[type=search]').attr('placeholder', 'Search');
                $('.dataTables_length select').addClass('form-control');
            })

            $('select.form-select').livequery(function () {
                $(this).select2();
            });

            $('.styled').livequery(function () {
                $(this).uniform({ radioClass: 'choice' });
            });

            $('.bootstrap-slider').livequery(function () {
                $(this).slider({
                    formatter: function (value) {
                        return 'Current value: ' + value;
                    }
                });
            });

            $('.file-styled').livequery(function () {
                $(this).uniform({
                    fileButtonHtml: '<i class="icon-upload"></i>',
                    wrapperClass: 'bg-primary'
                });
            });

            $('.html-editor').livequery(function () {
                $(this).summernote({
                    height: 150
                });
            })

            $('.html-air-editor').livequery(function () {
                $(this).summernote({
                    airMode: true
                });
            })

            $('[data-toggle="popover"]').livequery(function () {
                $(this).popover();
            })

            $('.form-group').livequery(function () {
                if ($(this).find('input[data-val-required],textarea[data-val-required],textarea[data-val-summernote],select[data-val-required]').not('[type="checkbox"]').length > 0) {
                    $(this).find('label').first().append('<span class="text-danger">&nbsp;*</span>');
                }
            });

            $('div.footer.hidden').livequery(function () {
                $(this).removeClass('hidden');
                $(this).appendTo('.content');
            });

            $('input.autocomplete').livequery(function () {
                $(this).removeClass('autocomplete')
                var source = $(this).attr('data-source');
                var multiple = $(this).attr('data-multiple') == 'true' ? true : false;
                $(this).select2({
                    multiple: multiple,
                    minimumInputLength: 0,
                    allowClear: true,
                    ajax: {
                        quietMillis: 150,
                        url: source,
                        dataType: 'json',
                        data: function (term, page) {
                            return {
                                i: page,
                                q: term
                            };
                        },

                        results: function (data, page) {
                            var _hasMoreResults = (page * 20) < data.Total;
                            return {
                                results: data.Results,
                                more: _hasMoreResults
                            };
                        }
                    }
                });

                var deafult = $(this).attr('data-select2-default');

                if (typeof deafult !== typeof undefined && deafult !== false) {
                    var _item = $.parseJSON($(this).attr('data-select2-default'));
                    $(this).select2('data', _item);
                }
            });


        }
    },
    Core: {
        RebindFormValidation: function () {
            $("form")
                .removeData("validator")
                .removeData("unobtrusiveValidation");

            $.validator
                .unobtrusive
                .parse("form");
        },
        HighlightCurrentMenuItem: function (item) {
            var _match = $('ul.navigation.navigation-main li a span:contains(' + item + ')');
            var _link = null;

            if (_match.length > 0) {
                _link = _match.parent().parent();
            } else {
                _link = $('ul.navigation.navigation-main li a:contains(' + item + ')').parent()
            }

            _link.addClass('active');
            _link.parent().attr('style', 'display: block;');
            _link.parent().parent().addClass('active');
        },


        //HighlightCurrentMenuItem: function (item) {
        //    $('ul.navigation.navigation-main li a span:contains(' + item + ')').parent().parent().addClass('active');
        //}
    },
    Notifications: {
        Show: function (_type, _msg) {
            switch ((_type)) {
                case 'Error':
                    toastr.error(_msg, 'Error')
                    break;
                case 'Success':
                    toastr.success(_msg, 'Success')
                    break;
                case 'Info':
                    toastr.info(_msg, 'Info')
                    break;
            }
        }
    }
}

$(document).ready(function () {
    WebApp.Init();
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "positionClass": "toast-top-center",
        "onclick": null,
        "showDuration": "6000",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "10000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

});