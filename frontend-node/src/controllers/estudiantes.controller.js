const axios = require('axios');
const API = process.env.API_BASE || 'http://localhost:5000';

exports.index = async (req, res) => {
  try {
    const [estRes, tipRes] = await Promise.all([
      axios.get(`${API}/api/estudiantes`),
      axios.get(`${API}/api/tipos-sangre`)
    ]);
    res.render('Estudiantes/index', {
      estudiantes: estRes.data,
      tipos: tipRes.data,
      error: req.query.error || ''
    });
  } catch (e) {
    res.status(500).send(e.message || 'Error cargando datos');
  }
};

exports.create = async (req, res) => {
  try {
    await axios.post(`${API}/api/estudiantes`, req.body);
    res.redirect('/estudiantes');
  } catch (e) {
    const msg = e.response?.data ?? e.message;
    res.redirect(`/estudiantes?error=${encodeURIComponent(
      typeof msg === 'string' ? msg : JSON.stringify(msg)
    )}`);
  }
};

exports.update = async (req, res) => {
  const id = req.params.id;
  try {
    await axios.put(`${API}/api/estudiantes/${id}`, req.body);
    res.redirect('/estudiantes');
  } catch (e) {
    const msg = e.response?.data ?? e.message;
    res.redirect(`/estudiantes?error=${encodeURIComponent(
      typeof msg === 'string' ? msg : JSON.stringify(msg)
    )}`);
  }
};

exports.remove = async (req, res) => {
  const id = req.params.id;
  try {
    await axios.delete(`${API}/api/estudiantes/${id}`);
    res.redirect('/estudiantes');
  } catch (e) {
    const msg = e.response?.data ?? e.message;
    res.redirect(`/estudiantes?error=${encodeURIComponent(
      typeof msg === 'string' ? msg : JSON.stringify(msg)
    )}`);
  }
};
