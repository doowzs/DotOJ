module.exports = {
  pathPrefix: `/DotOJ`,
  siteMetadata: {
    siteTitle: `问题求解OJ`,
    defaultTitle: `问题求解OJ`,
    siteTitleShort: `问题求解OJ`,
    siteDescription: `问题求解OJ的简陋文档`,
    siteUrl: `https://doowzs.github.io/DotOJ`,
    siteAuthor: `Tianyun Zhang`,
    siteImage: ``,
    siteLanguage: `zh_CN`,
    themeColor: `#8257E6`,
    basePath: `/`,
  },
  flags: { PRESERVE_WEBPACK_CACHE: true },
  plugins: [
    {
      resolve: `@rocketseat/gatsby-theme-docs`,
      options: {
        configPath: `src/config`,
        docsPath: `src/docs`,
        repositoryUrl: `https://github.com/doowzs/DotOJ`,
        baseDir: `Documentation`,
      },
    },
    {
      resolve: `gatsby-plugin-manifest`,
      options: {
        name: `Rocketseat Gatsby Themes`,
        short_name: `RS Gatsby Themes`,
        start_url: `/`,
        background_color: `#ffffff`,
        display: `standalone`,
        icon: `static/favicon.png`,
      },
    },
    `gatsby-plugin-sitemap`,
    // {
    //   resolve: `gatsby-plugin-google-analytics`,
    //   options: {
    //     trackingId: `YOUR_ANALYTICS_ID`,
    //   },
    // },
    `gatsby-plugin-remove-trailing-slashes`,
    {
      resolve: `gatsby-plugin-canonical-urls`,
      options: {
        siteUrl: `https://doowzs.github.io/DotOJ`,
      },
    },
    `gatsby-plugin-offline`,
  ],
};
