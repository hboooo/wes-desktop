var gulp = require('gulp');
var format = require('string-format');
var fs = require('fs');
var path = require('path');
var process = require('process');

var sourcePath = path.join(process.cwd(), 'Components\\{0}\\Wes.Customer.{0}\\obj\\x86\\Debug');
var targetPath = path.join(process.cwd(), 'setup\\debug\\Addins');
var customers = ['Mcc', 'Allsor', 'Avnet', 'Fitipower', 'Sinbon'];

gulp.task('publish-plugin', function (done) {
    customers.forEach(function (customer) {
        var fullPath = format(sourcePath, customer);
        if (fs.existsSync(fullPath)) {
            var dllFile = format('{}\\*.dll', fullPath);
            var destPath = path.join(targetPath, customer);
            gulp.src(dllFile)
                .pipe(gulp.dest(destPath))
            ;
            console.log(format('Copy [{}] dll files to [{}] dir', fullPath, destPath));
        } else {
            console.error(format('路径不存在, 请尝试首次编译生成此目录{}', fullPath));
        }
    });
    done();
});
