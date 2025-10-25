const axios = require('axios');
const API = process.env.API_BASE;

exports.index = async (req, res) => {
  try {
    const { data: tipos } = await axios.get(`${API}/api/tipos-sangre`);
    res.render('TiposSangre/index', { tipos });
  } catch (e) {
    console.error('Error listando tipos de sangre:', e.message);
    res.status(500).send('Error listando tipos de sangre.');
  }
};
