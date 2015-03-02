function tglDropMenuGrid(btn, unqKey, shiftLeft) {
    var menu_grid = "#menuGrid";

    var oldUnqKey = -1;
    if ($(menu_grid).find('.unqKey') != null) {
        oldUnqKey = $(menu_grid).find('.unqKey').val();
    }

    var copy_ul =  $(btn).siblings('ul').clone();
    $(menu_grid).html(copy_ul);
    $(menu_grid).find('ul').show();

    var offset = $(btn).offset();
    $(menu_grid).find('ul').css('top', offset.top + 33);
    $(menu_grid).find('ul').css('left', offset.left - 70 + shiftLeft);
    $(menu_grid).append('<input type="hidden" class="unqKey" value="' + unqKey + '" />');

    if ($(menu_grid).is(':visible')) {
        if (oldUnqKey == unqKey) {
            $(menu_grid).hide();
        }
    }
    else {
        $(menu_grid).show();
    }
}