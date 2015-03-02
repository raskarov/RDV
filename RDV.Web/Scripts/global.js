// Страница проинициализирована
$(document).ready(function () {

    // Компонент выбора дат
    $.datepicker.setDefaults($.datepicker.regional["ru"]);
    $(".datepicker").datepicker({
        dateFormat: "dd.mm.yy",
        changeMonth: true,
        changeYear: true
    });

    // Компонент таблиц
    $(".content-table tbody tr").each(function (index, item) {
        if (index % 2 != 0) {
            $(item).addClass("even");
        }
    });
    $(".content-table thead tr th input[type='checkbox']").change(function (e) {
        var val = $(this).attr("checked") == "checked";
        $(this).parents(".content-table").first().find("tbody tr td input[type='checkbox']").attr("checked", val).change();
    });

    // Автоматический скроллер экш меню
    $(window).scroll(function () {
        var menu = $("#action-menu");
        if (!menu.hasClass("unscroll")) {
            if (window.scrollY > 350) {

                $("#action-menu").css("margin-top", window.scrollY - 330);
            } else {
                $("#action-menu").css("margin-top", 0);
            }
        }
    });
    $(window).scroll();

    // Редактор клиентов
    if ($(".client-field").length > 0) {
        $(".client-field").autocomplete({
            minLength: 3,
            source: '/account/company/clients-autocomplete',
            select: function (event, ui) {
                if (ui.item.id == "-1") {
                    showNewClientDialog(this);
                } else {
                    $(this).val(ui.item.name);
                    $(this).parent().find("input[type='hidden']").val(ui.item.id);
                }
                return false;
            }
        }).data("autocomplete")._renderItem = function (ul, item) {
            return $("<li>")
                .data("item.autocomplete", item)
                .append("<a>" + item.name + "</a>")
                .appendTo(ul);
        };
    }

    // маска для телефонов
    //$(".phone-field").mask("9 (999) 999-99-99");

    // Проверка на совместимости со старыми браузерами
    if (!Modernizr.borderradius & !Modernizr.boxshadow & !Modernizr.localStorage) {
        $(".block-overlay").css("height", $(document).height()).css("width", $(document).width());
        $(".block-window").css("left", $(window).width() / 2 - 250).css("top", $(window).height() / 2 - 50);
        $(".obsolete-browser").show();
    }

    // Установка кнопки багрепорта вниз слева
    $(window).resize(function () {
        $("#bug-report").css("left", $("#header-clickable-wrapper").offset().left + 1060).css("top", window.innerHeight - $("#bug-report").outerHeight(true));
    }).resize();
});

// Возвращает идентификаторы выбранных элементов в таблице
function getSelectedTableItems() {
    var result = [];
    $(".content-table tbody tr input:checked").each(function (index, item) {
        result.push($(item).parent().parent().data("item-id"));
    });
    return result;
}

// Байндит обработчики таб контролов
function rebindTabs() {

}

/**
 * Глобальные объект, содержащий в себе все основные действия, доступные всему приложению
 * @constructor
 */
function Global() {

    /**
     * Загружает содержимое указанного контейнера с помощью Ajax, используя указанные параметры. На время зазрузки на объект накладывается полепрозрачный оверлей с индикатором загрузки
     * @param container - контейнер в котором загружать
     * @param method - тип запроса
     * @param url - урл из которого загружать
     * @param params - массив отсылаемых данных
     * @param callback - функция обратного вызова, вызывающаяся после завершения загрузки
     * @param showOverlay - отображать ли оверлей при работе
     */
    this.ajaxLoad = function (container, method, url, params, callback,showOverlay) {
        // Проверяем есть ли у нас оверлей для загрузок на странице
        var ajaxOverlay = $("#ajax-overlay");
        if (ajaxOverlay.length == 0) {
            ajaxOverlay = $("<div id='ajax-overlay'></div>");
            $("body").append(ajaxOverlay);
        }

        // Накладываем оверлей на контейнер
		if (showOverlay){
			var elementOffset = $(container).offset();
        	ajaxOverlay.css("left", elementOffset.left).css("top", elementOffset.top).width($(container).width()).height($(container).height()).show();
		}


        // Выполняем загрузку
        $.ajax({
            type: method,
            url: url,
            data: params,
            dataType: 'html',
            success: function (data) {
                $(container).html(data);
                ajaxOverlay.hide();
                if (callback != null) {
                    callback(data);
                }
            },
            failure: function (data) {
                ajaxOverlay.hide();
                alert("Ошибка в ходе загрузки данных");
            }
        });
    };
}
var global = new Global();