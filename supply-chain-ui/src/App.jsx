import React, { useState } from 'react';
import './App.css';

function App() {
  // Estados para capturar los datos del formulario transaccional
  const [productoId, setProductoId] = useState(1);
  const [almacenId, setAlmacenId] = useState(1);
  const [usuarioId, setUsuarioId] = useState(1);
  const [cantidad, setCantidad] = useState(1);
  const [tipoMovimiento, setTipoMovimiento] = useState('INGRESO');
  const [motivo, setMotivo] = useState('');

  // Estados para el manejo de alertas y consultas del stock dinámico
  const [feedback, setFeedback] = useState({ mensaje: '', esError: false });
  const [stockConsulta, setStockConsulta] = useState(null);

  // 🔑 CONEXIÓN BRINDADA: Apuntando directo al puerto real de tu backend .NET
  const API_URL = 'https://localhost:7047/api/movimientos';

  // Handler para despachar la transacción al Ledger de la API
  const handleRegistrar = async (e) => {
    e.preventDefault();
    setFeedback({ mensaje: '', esError: false });

    const payload = {
      productoId: parseInt(productoId),
      almacenId: parseInt(almacenId),
      usuarioId: parseInt(usuarioId),
      cantidad: parseInt(cantidad),
      tipoMovimiento,
      motivo
    };

    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
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
      setFeedback({ mensaje: 'No se pudo establecer conexión con el servidor Backend.', esError: true });
    }
  };

  // Handler para consultar el stock calculado históricamente
  const consultarStockActual = async () => {
    try {
      const response = await fetch(`${API_URL}/stock/${productoId}/${almacenId}`);
      const data = await response.json();
      setStockConsulta(data.stockDisponible);
    } catch (error) {
      console.error('Error al consultar stock', error);
    }
  };

  return (
    <div style={{ padding: '30px', fontFamily: 'Arial, sans-serif', maxWidth: '600px', margin: '0 auto', textAlign: 'left' }}>
      <h2>📦 Gestión de Cadena de Suministro (Ledger Logístico)</h2>
      <p style={{ color: '#666' }}>Ecosistema Conectado: React + .NET Core Web API + SQL Server</p>
      
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

      {/* Formulario de Operaciones */}
      <form onSubmit={handleRegistrar} style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        <label><b>ID Producto:</b></label>
        <input type="number" value={productoId} onChange={(e) => setProductoId(e.target.value)} required />

        <label><b>ID Almacén:</b></label>
        <input type="number" value={almacenId} onChange={(e) => setAlmacenId(e.target.value)} required />

        <label><b>ID Usuario (Operador):</b></label>
        <input type="number" value={usuarioId} onChange={(e) => setUsuarioId(e.target.value)} required />

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

      <hr style={{ margin: '30px 0' }} />

      {/* Panel de Consultas Rápidas en Memoria */}
      <div style={{ padding: '15px', backgroundColor: '#f8f9fa', borderRadius: '4px', border: '1px solid #dee2e6' }}>
        <h3>🔍 Consulta de Balance de Inventario</h3>
        <button type="button" onClick={consultarStockActual} style={{ padding: '6px 12px', cursor: 'pointer' }}>
          Calcular Stock Disponible
        </button>
        {stockConsulta !== null && (
          <p style={{ marginTop: '10px', fontSize: '18px' }}>
            Stock consolidado en Almacén {almacenId}: <b>{stockConsulta} unidades</b>
          </p>
        )}
      </div>
    </div>
  );
}

export default App;