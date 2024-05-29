// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    togglePasswordVisibiltyOnClick('password_eye', 'txtPassword');
    togglePasswordVisibiltyOnClick('password_eye2', 'txtPassword2');
});

function togglePasswordVisibiltyOnClick(imgId, inputId) {
    imgId = `#${imgId}`;
    inputId = `#${inputId}`;

    $(imgId).on('click', function (e) {
        if ($(imgId).hasClass('show')) {
            $(inputId).attr('type', 'text');
            $(imgId).removeClass('show')
                .attr('src', "/images/eye-closed-svgrepo-com.svg");
        }
        else {
            $(inputId).attr('type', 'password');
            $(imgId).addClass('show')
                .attr('src', "/images/eye-svgrepo-com.svg");
        }
    });
}

function togglePasswordVisibiltyOnHover(imgId, inputId) {
    imgId = `#${imgId}`;
    inputId = `#${inputId}`;

    $(imgId).on('mouseenter', function (e) {
        $(inputId).attr('type', 'text');
        $(imgId).removeClass('show')
            .attr('src', "/images/eye-closed-svgrepo-com.svg");
    }).on('mouseleave', function (e) {
        $(inputId).attr('type', 'password');
        $(imgId).addClass('show')
            .attr('src', "/images/eye-svgrepo-com.svg");
    });
}
