module.exports = {
  checkAuth: async (req, auth) => {
    if (process.env.CUBEJS_DEV_MODE === 'true') {
      return {};
    }
  },
  
  logger: (msg, params) => {
    console.log(`${msg}: ${JSON.stringify(params)}`);
  }
};