jQuery( document ).ready( function ( $ ) {

// site preloader -- also uncomment the div in the header and the css style for #preloader
    $( window ).load( function () {
        $( '#preloader' ).fadeOut( 'slow', function () {
            $( this ).remove();
        } );
    } );

    /* countdown */
    $( '.countdown' ).downCount( {
        date: '10/10/2017 12:00:00',
        offset: +10
    } );


    /* parallax background image http://www.minimit.com/articles/lets-animate/parallax-backgrounds-with-centered-content	
     /* detect touch */
    if ( "ontouchstart" in window ) {
        document.documentElement.className = document.documentElement.className + " touch";
    }
    if ( !$( "html" ).hasClass( "touch" ) ) {
        /* background fix */
        $( ".parallax" ).css( "background-attachment", "fixed" );
    }

    /* fix vertical when not overflow
     call fullscreenFix() if .fullscreen content changes */
    function fullscreenFix() {
        var h = $( 'body' ).height();
        // set .fullscreen height
        $( ".content-b" ).each( function ( i ) {
            if ( $( this ).innerHeight() <= h ) {
                $( this ).closest( ".fullscreen" ).addClass( "not-overflow" );
            }
        } );
    }
    $( window ).resize( fullscreenFix );
    fullscreenFix();

	getProgress();
	
	function getProgress(){
		$.get("https://dashboard.data-trading.com/api/dashboard/ico_status", function(response, status){
			"Calling callback"
			setProgressBar(response);
		});
	}

	function setProgressBar(progressbar_data){
		if(typeof progressbar_data !== 'undefined'){
			var html;
			var progress = progressbar_data.totalCoinsBought / progressbar_data.totalCoins * 100;

			$('.progress-holder .progress_percent_text').text(progress.toFixed(1)+'%');
			$('.progress-holder .progress_percent_value').attr('aria-valuenow',progress);
			$('.progress-holder .progress_percent_value').css('width',progress+'%');

			html = '<strong>'+numberWithCommas(progressbar_data.totalUsdInvested.toFixed(0)) + '</strong>  &nbsp;/&nbsp; '+numberWithCommas(progressbar_data.totalCoins * progressbar_data.tokenPrice)+' USD';

			$('.progress-holder .progress_amount_text').html(html);
		}
	}
	function numberWithCommas(x) {
		return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
	}

    /* resize background images */
    function backgroundResize() {
        var windowH = $( window ).height();
        $( ".landing, .last-tweet" ).each( function ( i ) {
            var path = $( this );
            // variables
            var contW = path.width();
            var contH = path.height();
            var imgW = path.attr( "data-img-width" );
            var imgH = path.attr( "data-img-height" );
            var ratio = imgW / imgH;
            // overflowing difference
            var diff = parseFloat( path.attr( "data-diff" ) );
            diff = diff ? diff : 0;
            // remaining height to have fullscreen image only on parallax
            var remainingH = 0;
            if ( path.hasClass( "parallax" ) && !$( "html" ).hasClass( "touch" ) ) {
                var maxH = contH > windowH ? contH : windowH;
                remainingH = windowH - contH;
            }
            // set img values depending on cont
            imgH = contH + remainingH + diff;
            imgW = imgH * ratio;
            // fix when too large
            if ( contW > imgW ) {
                imgW = contW;
                imgH = imgW / ratio;
            }
            //
            path.data( "resized-imgW", imgW );
            path.data( "resized-imgH", imgH );
            path.css( "background-size", imgW + "px " + imgH + "px" );
        } );
    }
    $( window ).resize( backgroundResize );
    $( window ).focus( backgroundResize );
    backgroundResize();

    /* set parallax background-position */
    function parallaxPosition( e ) {
        var heightWindow = $( window ).height();
        var topWindow = $( window ).scrollTop();
        var bottomWindow = topWindow + heightWindow;
        var currentWindow = ( topWindow + bottomWindow ) / 2;
        $( ".parallax" ).each( function ( i ) {
            var path = $( this );
            var height = path.height();
            var top = path.offset().top;
            var bottom = top + height;
            // only when in range
            if ( bottomWindow > top && topWindow < bottom ) {
                var imgW = path.data( "resized-imgW" );
                var imgH = path.data( "resized-imgH" );
                // min when image touch top of window
                var min = 0;
                // max when image touch bottom of window
                var max = -imgH + heightWindow;
                // overflow changes parallax
                var overflowH = height < heightWindow ? imgH - height : imgH - heightWindow; // fix height on overflow
                top = top - overflowH;
                bottom = bottom + overflowH;
                // value with linear interpolation
                var value = min + ( max - min ) * ( currentWindow - top ) / ( bottom - top );
                // set background-position
                var orizontalPosition = path.attr( "data-oriz-pos" );
                orizontalPosition = orizontalPosition ? orizontalPosition : "50%";
                $( this ).css( "background-position", orizontalPosition + " " + value + "px" );
            }
        } );
    }
    if ( !$( "html" ).hasClass( "touch" ) ) {
        $( window ).resize( parallaxPosition );
        //$(window).focus(parallaxPosition);
        $( window ).scroll( parallaxPosition );
        parallaxPosition();
    }

    $( 'a[href*=#]:not([href=#])' ).click( function () {
        if ( location.pathname.replace( /^\//, '' ) == this.pathname.replace( /^\//, '' ) || location.hostname == this.hostname ) {

            var target = $( this.hash );
            target = target.length ? target : $( '[name=' + this.hash.slice( 1 ) + ']' );
            if ( target.length ) {
                $( 'html,body' ).animate( {
                    scrollTop: target.offset().top
                }, 1000 );
                return false;
            }
        }
    } );


    new WOW().init();



//For Twitter posts
/*
    var config5 = {
        "id": '703291068796817408', //Change the id from here
        "domId": '',
        "maxTweets": 3,
        "enableLinks": true,
        "showUser": true,
        "showTime": true,
        "dateFunction": '',
        "showRetweet": false,
        "customCallback": handleTweets,
        "showInteraction": false
    };

    function handleTweets( tweets ) {
        var x = tweets.length;
        var n = 0;
        var element = document.getElementById( 'tweets' );
        var html = '<ul>';
        while ( n < x ) {
            html += '<li>' + tweets[n] + '</li>';
            n++;
        }
        html += '</ul>';
        if ($('#tweets').length) {
            element.innerHTML = html;
        }
    }

    twitterFetcher.fetch( config5 );
*/   
} );