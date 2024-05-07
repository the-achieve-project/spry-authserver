const handler = require('serve-handler');
const chokidar = require('chokidar');
const http = require("http")
const { Server } = require('socket.io');


const port = 3000; // Change this to your desired port
const publicDir = './'; // Change this to the directory containing your HTML file


const server = http.createServer((request, response) => {
    // You pass two more arguments for config and middleware
    // More details here: https://github.com/vercel/serve-handler#options
    return handler(request, response);
});

const io = new Server(server);
io.on('connection', (socket) => {
    console.log('a user connected');
});


server.listen(3000, () => {
    console.log('Running at http://localhost:3000');
});

const watcher = chokidar.watch(publicDir + '/*.html', {
    ignored: /(^|[\/\\])\../, // ignore dotfiles
    persistent: true,
});

watcher.on('change', () => {
    console.log("changes")
    io.emit("reload")
});

