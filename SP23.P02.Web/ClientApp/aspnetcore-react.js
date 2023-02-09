// This script configures the .env.development.local file with additional environment variables to configure CRA to use HTTPS
// The certificate to be used is from ASP.NET Core so it is trusted by your computer - this is important so that the site works well during development
// This is based on the asp.net core react.js template - modified so that it all sits in a single script and only does stuff needed for SELU CMPS 383

const fs = require("fs");
const path = require("path");
const spawn = require("child_process").spawn;

if (fs.existsSync(".env.development.local")) {
  console.log("Certificates already configured - delete .env.development.local if you want to regenerate");
  process.exit(0);
}

const baseFolder = !!process.env.APPDATA
  ? `${process.env.APPDATA}/ASP.NET/https`
  : !!process.env.HOME
  ? `${process.env.HOME}/.aspnet/https`
  : (() => {
      console.error("Could not locate a folder to store your SSL certificates in");
      process.exit(-1);
    })();

const certificateArg = process.argv.map((arg) => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {
  console.error(
    "Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly."
  );
  process.exit(-1);
}

const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  spawn("dotnet", ["dev-certs", "https", "--export-path", certFilePath, "--format", "Pem", "--no-password"], {
    stdio: "inherit",
  }).on("exit", (code) => {
    if (code === 0) {
      writeLocalEnv();
    } else {
      console.error("Failed to create your certificate - see errors from dotnet");
    }
    process.exit(code);
  });
} else {
  writeLocalEnv();
}

function writeLocalEnv() {
  fs.writeFileSync(
    ".env.development.local",
    `BROWSER=none
HTTPS=true
SSL_CRT_FILE=${certFilePath}
SSL_KEY_FILE=${keyFilePath}
`
  );
}
