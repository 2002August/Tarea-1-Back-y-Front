require('dotenv').config();
const express = require('express');
const path = require('path');

const app = express();

console.log('API_BASE =>', process.env.API_BASE);

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '..', 'views'));

app.use(express.urlencoded({ extended: true }));
app.use(express.json());

// estáticos (si los usas)
app.use('/public', express.static(path.join(__dirname, '..', 'public')));

// rutas
app.use('/estudiantes', require('./routes/estudiantes.routes'));
app.use('/tipos-sangre', require('./routes/tipos.routes'));

app.get('/', (req, res) => res.redirect('/estudiantes'));

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => console.log(`Frontend Node escuchando en http://localhost:${PORT}`));
