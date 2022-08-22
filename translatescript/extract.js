const { readFileSync, writeFile } = require("fs");
const async = require("async");
const { glob } = require("glob");
let types = require("./types.js");

function escape(text) {
  return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
}

let strings = [];
glob("**/*.{cs,js}", { cwd: "../LSCC" }, function (er, files) {
  async
    .eachSeries(files, function (file, nextFiles) {
      let script = readFileSync(`${process.cwd()}/../LSCC/${file}`).toString(
        "utf-8"
      );

      async
        .eachSeries(types, function (type, nextTypes) {
          console.log(`=> Processing ${file}\n`, type);

          let regex = new RegExp(
            `(?<=${escape(type.start)})(.*)(?=${escape(type.end)})`,
            "g"
          );

          let matches = Array.from(script.matchAll(regex)).map(
            (match) => type.start + match[0] + type.end
          );

          strings = strings.concat(matches);

          console.log(`=> Done processing, Found ${matches.length} strings.\n`);
          nextTypes();
        })
        .then(() => nextFiles());
    })
    .then(() => {
      console.log(
        `Checked Files: Total: ${files.length} JS: ${
          files.filter((f) => f.endsWith(".js")).length
        } CS: ${files.filter((f) => f.endsWith(".cs")).length} | Found ${
          strings.length
        } strings.`
      );
      writeFile(
        "./output/strings.js",
        `module.exports = ${JSON.stringify(strings)};`,
        function (err) {
          if (err) {
            return console.log(err);
          }
          console.log("The file was saved!");
        }
      );
    });
});
