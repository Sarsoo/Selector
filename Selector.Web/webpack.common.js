const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    entry: {
        app: './scripts/index.ts',
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                loader: 'ts-loader',
                exclude: /node_modules/,
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin()
    ],
    resolve: { extensions: ["*", ".js", ".jsx", ".ts", ".tsx"] },
    output: {
        filename: '[name].bundle.js',
        path: path.resolve(__dirname, 'wwwroot/js')
    }
};