$(document).ready(function () {
    $('.textarea_content').each(function (index) {
        $(this).attr('id', 'Content ');
        CKEDITOR.replace(this);
    });
})