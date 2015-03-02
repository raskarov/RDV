/**
 * Скроллер портфолио, обрабатывающий прокрутку элементов портфолио
 * @param config - конфигурационный объект, в котором должны быть заданы следующие свойства объектами jQuery
 *                 .overview - контейнер, внутри которого будет происходить прокрутка
 *                 .viewport - сам контейнер содержащий все прокручиваемые элементы
 *                 .nextBtn - кнопка прокручивающая вправо
 *                 .prevBtn - кнопка, прокручивающая влево
 *                 .paginator - блок содержащий paginator
 */
function portfolioScroller(config) {

    // Запоминаем себя
    var self = this;

    // Вычисляем требуемые ширины
    var scrollStep = $(config.viewport).children().first().outerWidth(true)+5;
    var overviewWidth = $(config.overview).width();
    var viewportWidth = $(config.viewport).children().length * scrollStep;

    // Устанавливаем ширину для прокручиваемого вьюпорта
    $(config.viewport).width(viewportWidth);
    var animationInProgress = false;

    // баайндим кнопки вправо влево
    $(config.prevBtn).click(function () {
        // Проверяем что мы можем смещаться назад
        var currentOffset = $(config.viewport).position().left;
        var invisibleItems = $(config.viewport).children().length - 5;
        if (invisibleItems < 0) {
            invisibleItems = 0;
        }
        if (viewportWidth - overviewWidth - currentOffset - scrollStep * invisibleItems > 25 && !animationInProgress) {
            var newOffset = currentOffset + scrollStep;
            animationInProgress = true;
            $(config.viewport).stop(true, true).animate({ "left": newOffset }, "normal", function () {
                animationInProgress = false;
                self.updatePaginatorPage();

            });
        }
    });
    $(config.nextBtn).click(function () {
        // Проверяем что мы можем смещаться вперед
        var currentOffset = $(config.viewport).position().left;
        if (viewportWidth - overviewWidth + currentOffset > 25 && !animationInProgress) {
            var newOffset = currentOffset - scrollStep;
            animationInProgress = true;
            $(config.viewport).stop(true, true).animate({ "left": newOffset }, "normal", function () {
                animationInProgress = false;
                self.updatePaginatorPage();
            });
        }
    });

    // Байндим ссылки пагинатора
    $("[page]", config.paginator).click(function (e) {
        $("[page]", config.paginator).removeClass("active");
        var page = parseInt($(this).attr("page"));
        var newOffset = page * scrollStep * 3 * -1;
        animationInProgress = true;
        $(config.viewport).stop(true, true).animate({ "left": newOffset }, "normal", function () {
            animationInProgress = false;
        });
        $(this).addClass("active");
        $(config.paginator).data("current-page", page);
        return false;
    });
    $("#next-link", config.paginator).click(function (e) {
        var currentPage = parseInt($(config.paginator).data("current-page"));
        var totalPages = parseInt($(config.paginator).data("total-pages"));
        if (currentPage == totalPages - 1) {
            return false;
        }
        var nextPage = currentPage + 1;
        $("[page='" + nextPage + "']", config.paginator).click();
        return false;
    });
    $("#prev-link", config.paginator).click(function (e) {
        var currentPage = parseInt($(config.paginator).data("current-page"));
        if (currentPage == 0) {
            return false;
        }
        var nextPage = currentPage - 1;
        $("[page='" + nextPage + "']", config.paginator).click();
        return false;
    });
    // Обновляет активную страницу в пагинаторе во время прокрутки
    this.updatePaginatorPage = function () {
        
    };
}