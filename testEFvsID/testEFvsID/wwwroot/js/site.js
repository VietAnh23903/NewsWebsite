$(document).ready(function () {

    var lastScrollTop = 0;
    $(window).scroll(function () {

        var scrollPosition = $(window).scrollTop();
        var listHeight = $(".list-hot-news").height();

        if (scrollPosition >= 50) {
            $(".header-bottom").addClass("fixed-top");
            $(".content").css("margin-top", "66px");
            $(".header-bottom .container").css("height", "36px");
            $(".show-list-menu").css("top", "36px");
        } else {

            $(".header-bottom").removeClass("fixed-top");
            $(".content").css("margin-top", "30px");
            $(".header-bottom .container").css("height", "50px");
            $(".show-list-menu").css("top", "50px");
        }


        var scrollDirection = (scrollPosition > lastScrollTop) ? 'down' : 'up';


        //if (scrollDirection === 'down' && scrollPosition >= listHeight + 50) {
        //    $(".list-hot-news").animate({ bottom: "100px" });
        //} else {
        //    $(".list-hot-news").animate({ bottom: "0px" });
        //}


        lastScrollTop = scrollPosition;
    });

    $(".show-list-category").click(function () {
        $(".model-list-category").animate({
            height: "toggle"
        })
    })
    $('.search-text').focusin(function () {
        $('#search-result').css("display", "inline-block");
        
        $('.search-text').on('input', function () {
            var newValue = $(".search-text").val();
            $.ajax({

                url: '/Home/Search', 
                method: 'GET', 
                data: { search: newValue }, 
                contentType: 'application/json;charset=utf-8',
                
                success: function (response) {
                    if (response.result == true) {
                        var htmlContent = "";
                        response.data.forEach(function (p) {
                            htmlContent +=
                                "<div class=" + "row" + ">" +

                                "<img class=" + "col-4" + " src='" + p.thumbnail + "' style=" + "height:100%;" + "width:100%;" + "object-fit:cover;" + ">" +
                                "<a  href='/" + p.slug + ".html' class=" + "col-8" + " style=" + "font-size:12px;" + ">" + p.title + "</a>" +
                                "</div>";
                        });
                        $('#search-result').html(htmlContent);
                        
                    }
                    else {
                        var htmlContent = "";
                        htmlContent += "<div>" + response.mess + "</div>";
                        $('#search-result').html(htmlContent);
                        
                    }
                    
                },
                error: function (xhr, status, error) {
                   
                    console.error(error);
                }
            });

        });
    });
    $('.search-text').focusout(function () {
        $('#search-result').css("display", "none");
    });

    $('#search-result').hover(
        function () {
        $('#search-result').css("display", "inline-block");
        }
    );
    $("#btnSearch").click(function () {
        var keyword = $(".search-text").val();
        var listkey = localStorage.getItem('keywordss') ? JSON.parse(localStorage.getItem('keywordss')) : [];
        if (!listkey.includes(keyword)) {
            listkey.push(keyword);
            localStorage.setItem('keywordss', JSON.stringify(listkey));     
        }
                   
        
    });
    var displaylistkey = JSON.parse(localStorage.getItem('keywordss'));
    var keywordContent = "";
    displaylistkey.forEach(function (k) {
        keywordContent +=
            "<a herf=" + "/search/" + k + ">" +
        "<span class=" + "badge badge-pill badge-light" + ">" + "#" + k +

        "</span>"
        +"</a>"
           ;
        $("#search-keyword").html(keywordContent);
    })

    
    function getPageWidth() {
        var pageWidth = $(window).width();
        $(".show-list-menu").css("width", pageWidth);
    }
    getPageWidth();
    $(window).resize(function () {
        getPageWidth();
    });
   
    //$(window).animate({
    //    scrollTop: distance
    //}, 'slow');
    var cate = $(".isCate").val();
    var tt = $(".tt").val();
    if (cate != "") {
        var div = $(".checkpoint");
        var distance = div.offset().top;


        $('html').animate({
            scrollTop: distance-55
        }, 1500);
    }
    
    $(".show-list-category").click(function () {
        $(".show-list-menu").fadeToggle("slow");
    })

    $(".list-custom-item").mousemove(function (e) {
        
        console.log("X: " + e.pageX + ", Y: " + e.pageY);
        $('.sub-info').css({
            top: e.pageY-100,
            left: e.pageX+15
        });
        $('.comboshow').mouseenter(function () {

            //$(this).css("color", "red");
            $(this).find(".sub-info").show();
            //console.log($(this).next(".sub-info").show());
            }).mouseleave(function () {
                $(this).find(".sub-info").fadeOut();
               
            });
    });
    
})
