module.exports = {
  apps: [
    {
      name: "jsscreenshotserver",
      script: "./index.js",
      max_restarts: "10",
      out_file: "/var/www/rncjsadminscreenshotserverapi.ls-cc.info/console.log",
      log_date_format: "YYYY-MM-DD HH:mm Z",
    },
  ],
};
