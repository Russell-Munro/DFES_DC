﻿/// <binding />
const Webpack = require("webpack");
const Path = require("path");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCssAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const AssetsPlugin = require("assets-webpack-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const BundleAnalyzerPlugin = require("webpack-bundle-analyzer")
  .BundleAnalyzerPlugin;

var reverseProxy = "localhost:5000"; //IIS or IIS express must be running on this address
// run the dot net app from its project root with dotnet watch run


module.exports = env => {
  // Constants for environment/mode configurations
  const { PLATFORM, STATS, PORT, SITE } = env;

  const nodeEnv = env.NODE_ENV;
  const isProduction = PLATFORM === "production";
  const isDebug = PLATFORM === "development";
  const isWatch = nodeEnv === "local";
  const showStats = STATS === "show";

  console.log("isProduction", isProduction ? "true" : "false");
  console.log("isDebug", isDebug ? "true" : "false");
  console.log("isWatch", isWatch ? "true" : "false");

  const plugins = [
    // Make the WebpackHelper.cs magic work - include the files with hashes
    new AssetsPlugin({
      filename: "webpack.assets.json",
      // path: "assets/dist",
      path: "wwwroot",

      prettyPrint: true,
      metadata: {
        ProductionBuild: isProduction,
        WatchBuild: isWatch
      }
    }),

    // Automatically load modules
    new Webpack.ProvidePlugin({
      $: "jquery",
      jQuery: "jquery",
      "window.jQuery": "jquery",
      "window.$": "jquery"
    })
  ];

  if (isProduction) {
    plugins.push(
      new MiniCssExtractPlugin({
        filename: "[name].css",
//        filename: "[name].[hash].css",
        chunkFilename: "[id].css"
      })
    );

    plugins.push(new CleanWebpackPlugin());

    plugins.push(new OptimizeCssAssetsPlugin());
  }

  if (isDebug) {
    // Add console progress bar
    plugins.push(new Webpack.ProgressPlugin());
  }

  if (showStats) {
    // Generates visual representation of bundle sizes
    plugins.push(new BundleAnalyzerPlugin());
  }

  return {
    // The folder to base file paths on as its root
    context: Path.resolve(__dirname, "assets"),

    entry: {
      // Project
      // top: './src/bundleTop.js',
      // bottom: './src/bundleBottom.js',
      
      connections: ["babel-polyfill","./src/index.js"]
    },

    output: {
      filename: chunkData => {
        if (isProduction) {
//          return "[name]-bundle.[hash].js";
          return "[name]-bundle.js";
        } else {
          return "[name]-bundle.js";
        }
      },
      // Use contenthash for vendor packages, so that these are only re-downloaded when the content changes
      chunkFilename: "[name].js",
      // path: Path.resolve(__dirname, "assets/dist"),
      // publicPath: "/assets/dist"
      path: Path.resolve(__dirname, "wwwroot"),
      publicPath: ""
    },

    module: {
      rules: [
        // Config in .babelrc
        {
          test: /.(js|jsx)$/,
          exclude: /(node_modules|bcore-js\b|@babel\b)/,

          use: {
            loader: "babel-loader",
            options: {
			  presets: [
                    "@babel/preset-env","@babel/preset-react"],
			  "plugins": [
				"@babel/plugin-proposal-class-properties"
			  ],
              sourceMap: true
            }
          }
        },
        {
          test: /\.(sa|sc|c)ss$/,
          use: [
            // Serve CSS via JavaScript while debugging
            isProduction
              ? { loader: MiniCssExtractPlugin.loader }
              : { loader: "style-loader" },
            {
              loader: "css-loader",
              options: { sourceMap: isDebug, importLoaders: 3 }
            },
            // Refer to postcss.config.js for more
            { loader: "postcss-loader", options: { sourceMap: isDebug } },
            { loader: "sass-loader", options: { sourceMap: isDebug } }
          ]
        },
        // https://www.npmjs.com/package/webpack-modernizr-loader
        // Config in .modernizrrc.js
        {
          test: /\.modernizrrc\.js$/,
          loader: "webpack-modernizr-loader"
        },
        {
          test: /\.(gif|svg|jpg|png)$/,
          loader: "file-loader"
        },
        // Automatically optimises SVGs for inline conversion
        {
          test: /\.svg$/,
          use: [
            { loader: "file-loader" },
            {
              loader: "svgo-loader",
              options: {
                plugins: [{ removeViewBox: false }, { removeDimensions: true }]
              }
            }
          ]
        },
        {
          test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
          use: [
            {
              loader: 'file-loader',
              options: {
                name: '[name].[ext]',
                outputPath: '/fonts/'
              }
            }
          ]
        }
      ]
    },

    optimization: {
      minimizer: [
        new TerserPlugin({
          extractComments: true, // Summarises comments into one file
          terserOptions: {
            warnings: false,
            parse: {},
            compress: {
              drop_console: true
            },
            mangle: true, // Note `mangle.properties` is `false` by default.
            output: null,
            toplevel: false,
            nameCache: null,
            ie8: false,
            keep_fnames: false
          }
        })
      ],
      splitChunks: isProduction
        ? {
            chunks: "all",
            name: true
          }
        : {}
    },

    plugins: plugins,

    resolve: {
      extensions: ["*", ".js", ".jsx"],
      alias: {
        modernizr$: Path.resolve(__dirname, ".modernizrrc.js")
      }
    },

    // Pretty console
    stats: {
      colors: true
    },

    // Build source map for debug
    devtool: isDebug ? "eval-source-map" : "none",

    watchOptions: {
      aggregateTimeout: 300,
      poll: 1000,
      ignored: /node_modules/
    },

    devServer: isWatch
      ? {
          // Match the output path
          contentBase: Path.resolve(__dirname, "assets/dist"),

          // Match the output publicPath
          //publicPath: "/assets/dist",
          publicPath:"",
          watchContentBase: true,
          proxy: {
            "*": {
              target: "http://" + reverseProxy,
              secure: false
            }
          },
          port: 5005,
          host: "localhost"
        }
      : {}
  };
};
