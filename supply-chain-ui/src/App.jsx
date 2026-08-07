import { useState, useEffect } from 'react';
import './App.css';
// 📊 Plataforma de analítica y KPIs empresariales
import AnalyticsDashboard from './components/AnalyticsDashboard';
import Login from './components/Login';
import { apiFetch, leerSesion, cerrarSesion } from './api';

function App() {
  // Sesión activa (null = no autenticado). Se rehidrata desde localStorage.
  const [sesion, setSesion] = useState(() => leerSesion());

  // Estados para capturar los datos del formulario transaccional.
  // Ya no hay campo de usuario: la autoría del movimiento la aporta el token.
  const [productoId, setProductoId] = useState(1);
  const [almacenId, setAlmacenId] = useState(1);
  const [cantidad, setCantidad] = useState(1);
  const [tipoMovimiento, setTipoMovimiento] = useState('INGRESO');
  const [motivo, setMotivo] = useState('');

  // Estados para el manejo de alertas y consultas del stock dinámico
  const [feedback, setFeedback] = useState({ mensaje: '', esError: false });
  const [stockConsulta, setStockConsulta] = useState(null);

  const esAdmin = sesion?.rol === 'Admin';

  // Si el token expira a mitad de sesión, la API responde 401 y api.js emite
  // este evento: la app vuelve al login en lugar de fallar en silencio.
  useEffect(() => {
    const alExpirar = () => setSesion(null);
    window.addEventListener('sesion-expirada', alExpirar);
    return () => window.removeEventListener('sesion-expirada', alExpirar);
  }, []);

  const handleCerrarSesion = () => {
    cerrarSesion();
    setSesion(null);
  };

  // Handler para despachar la transacción al Ledger de la API
  const handleRegistrar = async (e) => {
    e.preventDefault();
    setFeedback({ mensaje: '', esError: false });

    const payload = {
      productoId: parseInt(productoId),
      almacenId: parseInt(almacenId),
      cantidad: parseInt(cantidad),
      tipoMovimiento,
      motivo
    };

    try {
      const response = await apiFetch('/api/movimientos', {
        method: 'POST',
        body: JSON.stringify(payload)
      });

      const data = await response.json();

      if (response.ok) {
        setFeedback({ mensaje: data.mensaje, esError: false });
        setMotivo(''); // Limpiar caja de texto de motivo tras un éxito
        consultarStockActual(); // Refrescar el contador de stock en pantalla
      } else {
        // Captura las excepciones de negocio lanzadas por .NET (Ej: Stock insuficiente)
        setFeedback({ mensaje: data.error || 'Error en la transacción.', esError: true });
      }
    } catch (error) {
      setFeedback({ mensaje: error.message, esError: true });
    }
  };

  // Handler para consultar el stock calculado históricamente
  const consultarStockActual = async () => {
    try {
      const response = await apiFetch(`/api/movimientos/stock/${productoId}/${almacenId}`);
      const data = await response.json();
      setStockConsulta(data.stockDisponible);
    } catch (error) {
      setFeedback({ mensaje: error.message, esError: true });
    }
  };

  if (!sesion) {
    return <Login onAutenticado={setSesion} />;
  }

  return (
    <div style={{ padding: '30px', fontFamily: 'Arial, sans-serif', maxWidth: '1000px', margin: '0 auto', textAlign: 'left' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '16px' }}>
        <div>
          <h2>📦 Gestión de Cadena de Suministro (Ledger Logístico)</h2>
          <p style={{ color: '#666' }}>Ecosistema Conectado: React + .NET Core Web API + SQL Server</p>
        </div>
        <div style={{ textAlign: 'right', fontSize: '13px', color: '#444', whiteSpace: 'nowrap' }}>
          <div><b>{sesion.nombreCompleto}</b></div>
          <div style={{ color: '#666' }}>Rol: {sesion.rol}</div>
          <button
            type="button"
            onClick={handleCerrarSesion}
            style={{ marginTop: '6px', padding: '4px 10px', cursor: 'pointer', fontSize: '12px' }}
          >
            Cerrar sesión
          </button>
        </div>
      </div>

      <hr />

      {/* Banners dinámicos de respuesta (Verde = Éxito, Rojo = Control de Negocio) */}
      {feedback.mensaje && (
        <div style={{
          padding: '12px',
          borderRadius: '4px',
          marginBottom: '20px',
          backgroundColor: feedback.esError ? '#f8d7da' : '#d1e7dd',
          color: feedback.esError ? '#842029' : '#0f5132',
          fontWeight: 'bold'
        }}>
          {feedback.mensaje}
        </div>
      )}

      {/* Formulario de Operaciones — solo para Admin.
          Ocultarlo es comodidad de UI; quien lo fuerce igual recibe un 403
          del servidor, que es donde la regla se aplica de verdad. */}
      {esAdmin ? (
        <form onSubmit={handleRegistrar} style={{ display: 'flex', flexDirection: 'column', gap: '12px', maxWidth: '600px' }}>
          <label><b>ID Producto:</b></label>
          <input type="number" value={productoId} onChange={(e) => setProductoId(e.target.value)} required />

          <label><b>ID Almacén:</b></label>
          <input type="number" value={almacenId} onChange={(e) => setAlmacenId(e.target.value)} required />

          <label><b>Cantidad Absoluta:</b></label>
          <input type="number" min="1" value={cantidad} onChange={(e) => setCantidad(e.target.value)} required />

          <label><b>Tipo de Movimiento:</b></label>
          <select value={tipoMovimiento} onChange={(e) => setTipoMovimiento(e.target.value)} style={{ padding: '6px' }}>
            <option value="INGRESO">INGRESO (+ Stock)</option>
            <option value="SALIDA">SALIDA (- Stock)</option>
            <option value="MERMA">MERMA (- Stock por Daño)</option>
          </select>

          <label><b>Motivo de la Operación:</b></label>
          <input type="text" placeholder="Ej: Ingreso por orden de compra #102" value={motivo} onChange={(e) => setMotivo(e.target.value)} required />

          <button type="submit" style={{ padding: '12px', backgroundColor: '#0d6efd', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold', fontSize: '15px' }}>
            Ejecutar Transacción en Ledger
          </button>
        </form>
      ) : (
        <div style={{ padding: '15px', backgroundColor: '#fff3cd', color: '#664d03', borderRadius: '4px', border: '1px solid #ffecb5', maxWidth: '600px' }}>
          Tu rol (<b>{sesion.rol}</b>) tiene acceso de solo lectura. El registro de movimientos en el ledger está reservado al rol <b>Admin</b>.
        </div>
      )}

      <hr style={{ margin: '30px 0' }} />

      {/* Panel de Consultas Rápidas en Memoria */}
      <div style={{ padding: '15px', backgroundColor: '#f8f9fa', borderRadius: '4px', border: '1px solid #dee2e6', maxWidth: '600px' }}>
        <h3>🔍 Consulta de Balance de Inventario</h3>
        <button type="button" onClick={consultarStockActual} style={{ padding: '6px 12px', cursor: 'pointer' }}>
          Calcular Stock Disponible
        </button>
        {stockConsulta !== null && (
          <p style={{ marginTop: '10px', fontSize: '18px', color: '#333' }}>
            Stock consolidado en Almacén {almacenId}: <b>{stockConsulta} unidades</b>
          </p>
        )}
      </div>

      {/* 🚀 Dashboard Analítico Corporativo */}
      <AnalyticsDashboard />
    </div>
  );
}

export default App;
