"use strict";
const Path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
	entry: './React/index.jsx',
	plugins: [
		new MiniCssExtractPlugin({
			// Options similar to the same options in webpackOptions.output
			// both options are optional
			filename: "[name].css",
			chunkFilename: "[id].css"
		}),
		new CopyWebpackPlugin([{
			from: "React/translations/*.json",
			to: "translations",
			toType: "dir",
			flatten: true
		}], { debug: false })
	],
	module: {
		rules: [
			{
				test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                include: /React/,
				use: {
					loader: "babel-loader",
					options: {
						presets: [
							["@babel/preset-env", {
								"targets": {
									"safari": "10.1",
									"firefox": "60",
									"chrome": "65",
									"edge": "14"
								}
							}],
							"@babel/preset-react"]
					}
				}
			},
			{
				test: /\.less$/,
                exclude: /node_modules/,
                include: /React/,
				use: [{
					loader: MiniCssExtractPlugin.loader,
				}, {
					loader: "css-loader"
				}, {
					loader: "less-loader"
				}],
			}
		]
    },
    output: { path: Path.join(__dirname, './wwwroot/dist') },
	resolve: {
		extensions: ['.js', '.jsx']
	}
};