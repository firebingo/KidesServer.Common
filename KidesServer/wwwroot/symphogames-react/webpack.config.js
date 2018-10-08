const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
	entry: './src/index.jsx',
	plugins: [
		new MiniCssExtractPlugin({
			// Options similar to the same options in webpackOptions.output
			// both options are optional
			filename: "[name].css",
			chunkFilename: "[id].css"
		})
	],
	module: {
		rules: [
			{
				test: /\.(js|jsx)$/,
				exclude: /node_modules/,
				use: {
					loader: "babel-loader",
					options: {
						presets: ["@babel/preset-env", "@babel/preset-react"]
					}
				}
			},
			{
				test: /\.less$/,

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
	resolve: {
		extensions: ['.js', '.jsx']
	}
};