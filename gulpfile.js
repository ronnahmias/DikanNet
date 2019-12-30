const gulp = require('gulp'); // npm install --save-dev gulp
const imagemin = require('gulp-imagemin'); // npm install --save-dev gulp-imagemin
const uglify = require('gulp-uglify'); // npm install --save-dev gulp-uglify
const sass = require('gulp-sass'); // npm install --save-dev gulp-sass
const concat = require('gulp-concat') // npm install --save-dev gulp-concat
const rename = require('gulp-rename') // npm install --save-dev gulp-rename
const babel = require('gulp-babel');
/*
  -- TOP LEVEL FUNCTION --
  gulp.task - Define tasks
  gulp.src - Point tofiles to use
  fulp.dest - Points to folder to output
  gulp.watch - Watch files and folders for changes
*/

// folder of JavaScript Destintion
const destJs = 'DikanNetProject/Scripts/MyJs';

// Logs Message
function message(done){
  console.log('Gulp is running...');
  done();
}

// Error log
function errorLog(error){
  console.log(error.toString());
  this.emit('end');
  done();
}

// Copy All HTML files
function copyHtml(done){
  gulp.src('src/*.html')
      .pipe(gulp.dest('dist'));
  done();
}

// Optimize Images
function imageMin(done){
    gulp.src('DikanNetProject/src/pic/*')
      .pipe(imagemin())
      .on('error',errorLog)
      .pipe(gulp.dest('DikanNetProject/Content/Pic'));
  done();
}

// Minify JS
function minifyJs(done)
{
  gulp.src('DikanNetProject/src/js/*.js')
      .pipe(uglify())
      .pipe(rename({
        suffix: '.min'
      }))
      .pipe(gulp.dest(destJs));
  done();
}

// CopyOrginal JS
function copyJs(done)
{
    gulp.src('DikanNetProject/src/js/*.js')
      .pipe(gulp.dest(destJs));
      done();
}

// Concat scripts
function combainJs(done)
{
  gulp.src('DikanNetProject/src/js/*.js')
      .pipe(concat('main.js'))
      .pipe(uglify())
      .pipe(gulp.dest(destJs));
  done();
}

//From ES6 to regular js
function convertES6(done) {
  gulp.src('DikanNetProject/src/js/ES6/*.js')
      .pipe(babel())
      .pipe(gulp.dest('DikanNetProject/src/js/'));
  done();
}


// Compile sass
function sassToCss(done){
  gulp.src('DikanNetProject/src/sass/*.scss')
      .pipe(sass().on('error',errorLog))
      .pipe(gulp.dest('DikanNetProject/Content/Css'));
  done();
}

// Watch files
function watch_files() {
  gulp.watch('DikanNetProject/src/sass/*.scss',sassToCss );
  // gulp.watch('DikanNetProject/src/js/ES6/*.js' ,convertES6 );
  gulp.watch('DikanNetProject/src/js/*.js' ,minifyJs );
  gulp.watch('DikanNetProject/src/js/*.js' ,copyJs );
  gulp.watch('DikanNetProject/src/pic/*' ,imageMin );
}

gulp.task('message', message);
gulp.task('copyHtml',copyHtml);
gulp.task('imageMin',imageMin);
gulp.task('minifyJs',minifyJs);
gulp.task('copyJs',copyJs);
gulp.task('sassToCss',sassToCss);
gulp.task('combainJs',combainJs);
// gulp.task('convertES6',convertES6);

gulp.task('default', gulp.parallel(message,imageMin,copyJs,minifyJs,sassToCss) );
gulp.task('watch',gulp.series(watch_files));
