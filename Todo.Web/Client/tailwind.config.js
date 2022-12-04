const colors = require('tailwindcss/colors');
const plugin = require('tailwindcss/plugin');

module.exports = {
  content: [
      "./**/*.razor",
      "./../Todo.Web.Server/Pages/_Host.cshtml"
    ],
  theme: {
    extend: {},
  },
  plugins: [
      plugin(function({ addBase, theme }) { // https://github.com/tailwindlabs/tailwindcss/blob/master/stubs/defaultConfig.stub.js
            addBase({
                'h1': {
                    fontSize: 'calc(1.375rem + 1.5vw)',
                    fontWeight: theme('fontWeight.bold'),
                    paddingBottom: theme('spacing.4'),
                    lineHeight: theme('lineHeight.tight'),
                },
                'h2': {
                    fontSize: 'calc(1.325rem + .9vw)',
                    fontWeight: theme('fontWeight.bold'),
                    paddingBottom: theme('spacing.4'),
                    lineHeight: theme('lineHeight.tight'),
                },
                'h3': {
                    fontSize: 'calc(1.3rem + .6vw)',
                    fontWeight: theme('fontWeight.bold'),
                    paddingBottom: theme('spacing.4'),
                    lineHeight: theme('lineHeight.leadingTight'),
                },
                'h4': {
                    fontSize: 'calc(1.275rem + .3vw)',
                    fontWeight: theme('fontWeight.bold'),
                    paddingBottom: theme('spacing.1'),
                    lineHeight: theme('lineHeight.leadingTight'),
                },
                'h5': {
                    fontSize: '1.25rem',
                    fontWeight: theme('fontWeight.bold'),
                    paddingBottom: theme('spacing.1'),
                },
                'h6': {
                    fontSize: '1rem',
                    fontWeight: theme('fontWeight.bold'),
                    paddingBottom: theme('spacing.1'),
                },
                'a': {
                    color: theme('colors.link')
                },
                'picture': {
                    width: theme('width.full')
                },
                'p': {
                    marginBottom: theme('spacing.4')
                },
            })
      })
    ],
}
