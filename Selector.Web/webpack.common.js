const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    entry: {
        now: './scripts/now.ts',
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                loader: 'ts-loader',
                options: {
                    appendTsSuffixTo: [/\.vue$/],
                },
                exclude: /node_modules/,
            },
            {
                test: /\.vue$/,
                loader: 'vue-loader',
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin()
    ],
    resolve: { 
        extensions: ["*", ".js", ".jsx", ".ts", ".tsx", ".vue"] ,
        alias: {
            vue: "vue/dist/vue.esm-bundler.js"
        }
    },
    output: {
        filename: '[name].bundle.js',
        path: path.resolve(__dirname, 'wwwroot/js')
    }
};