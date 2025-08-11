var autoprefixer = require('autoprefixer');
var postcssImport = require('postcss-import');

//if autoprefixer causes issue this one can be switched in
//var postcssPresetEnv = require('postcss-preset-env'); 

module.exports = {		
	sourceMap: true,
	plugins: [
		//apply prefixes for other browsers
		autoprefixer({
			remove: false
		}),
		postcssImport({

		})		
	]

}