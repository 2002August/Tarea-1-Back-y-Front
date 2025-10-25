const router = require('express').Router();
const c = require('../controllers/estudiantes.controller');

// Lista
router.get('/', c.index);

// Si alguien navega a /estudiantes/:id por GET, redirige a la lista
router.get('/:id', (req, res) => res.redirect('/estudiantes'));

// CRUD
router.post('/', c.create);            // crear
router.post('/:id', c.update);         // actualizar
router.post('/:id/delete', c.remove);  // eliminar

module.exports = router;
