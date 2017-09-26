/// <binding AfterBuild='default' ProjectOpened='watch' />
var gulp = require('gulp'),
    watch = require('gulp-watch'),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    minify = require('gulp-clean-css'),
    sass = require('gulp-sass'),
    pump = require('pump');

var paths = {
    webroot: "./wwwroot/"
    
};

paths.js += paths.webroot + "js/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.ts = paths.webroot + "src/**/*.ts";
paths.scss = paths.webroot + "scss/*.scss";

gulp.task('min:site.css',
    function() {
        return gulp.src(paths.webroot+"css/site.css")
            .pipe(concat('site.min.css'))
            .pipe(minify({ debug: true },
                function(details) {
                    console.log(details.name + ': ' + details.stats.originalSize);
                    console.log(details.name + ': ' + details.stats.minifiedSize);
                }))
            .pipe(gulp.dest(paths.webroot+"css"));
    });
gulp.task('scss',
    function() {
        return gulp.src(paths.scss)
            .pipe(sass().on('error', sass.logError))
            .pipe(gulp.dest(paths.webroot + 'css'));
    });


gulp.task('scripts',
    function(cb) {
        pump([
                gulp.src(paths.js),
                
                uglify(),
                gulp.dest(paths.webroot+'js')
            ],
            cb
        );
    });

gulp.task('watch',
    function() {
        gulp.watch(paths.scss, ['scss', 'min:site.css']);
        gulp.watch(paths.webroot + "css/site.css", ['min:site.css']);
        gulp.watch(paths.script, ['scripts']);
    });
gulp.task("default", ["scripts", "scss"]);