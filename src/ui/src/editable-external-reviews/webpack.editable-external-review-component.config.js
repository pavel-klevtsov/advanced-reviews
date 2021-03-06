var path = require("path");

module.exports = (env, argv) => {
    const webpackCommon = require("../../webpack.config.common")(env, argv);

    webpackCommon.entry = "./src/editable-external-reviews/editable-external-review-component.tsx";

    webpackCommon.output = {
        filename: "external-review-component.js",
        //libraryTarget: "amd",
        //libraryExport: "default",
        path: path.resolve(__dirname, "../../../alloy/modules/_protected/advanced-cms.ExternalReviews/Views")
    };

    return webpackCommon;
};
