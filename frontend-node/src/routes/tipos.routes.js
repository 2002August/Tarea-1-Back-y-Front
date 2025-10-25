const router = require('express').Router();
const c = require('../controllers/tipos.controller');

router.get('/', c.index);

module.exports = router;
