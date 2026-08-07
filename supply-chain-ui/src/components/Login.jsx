import { useState } from 'react';
import { login } from '../api';

const styles = {
  fondo: { minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: '#111827', fontFamily: 'Arial, sans-serif' },
  tarjeta: { backgroundColor: '#1f2937', color: '#f3f4f6', padding: '32px', borderRadius: '12px', width: '100%', maxWidth: '380px', boxShadow: '0 10px 25px rgba(0,0,0,0.5)' },
  titulo: { fontSize: '20px', marginBottom: '4px' },
  subtitulo: { color: '#9ca3af', fontSize: '13px', marginBottom: '24px' },
  etiqueta: { display: 'block', fontSize: '13px', fontWeight: 'bold', marginBottom: '6px' },
  input: { width: '100%', padding: '10px', marginBottom: '16px', borderRadius: '6px', border: '1px solid #4b5563', backgroundColor: '#111827', color: '#f3f4f6', fontSize: '14px' },
  boton: { width: '100%', padding: '12px', backgroundColor: '#2563eb', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', fontSize: '15px' },
  error: { backgroundColor: '#7f1d1d', color: '#fecaca', padding: '10px', borderRadius: '6px', marginBottom: '16px', fontSize: '13px' },
  ayuda: { marginTop: '20px', paddingTop: '16px', borderTop: '1px solid #374151', fontSize: '12px', color: '#9ca3af', lineHeight: 1.6 }
};

export default function Login({ onAutenticado }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const manejarEnvio = async (e) => {
    e.preventDefault();
    setError('');
    setCargando(true);

    try {
      const sesion = await login(email, password);
      onAutenticado(sesion);
    } catch (err) {
      setError(err.message);
    } finally {
      setCargando(false);
    }
  };

  return (
    <div style={styles.fondo}>
      <form style={styles.tarjeta} onSubmit={manejarEnvio}>
        <h2 style={styles.titulo}>📦 SupplyChainCore</h2>
        <p style={styles.subtitulo}>Ledger logístico — acceso restringido</p>

        {error && <div style={styles.error}>{error}</div>}

        <label style={styles.etiqueta} htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          style={styles.input}
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          autoComplete="username"
          required
        />

        <label style={styles.etiqueta} htmlFor="password">Contraseña</label>
        <input
          id="password"
          type="password"
          style={styles.input}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
          required
        />

        <button type="submit" style={styles.boton} disabled={cargando}>
          {cargando ? 'Verificando…' : 'Iniciar sesión'}
        </button>

        <div style={styles.ayuda}>
          <b>Cuentas de demostración:</b><br />
          jose@supplychain.com / Admin123! — <i>Admin, puede registrar movimientos</i><br />
          operador@supplychain.com / Operador123! — <i>Operador, solo lectura</i>
        </div>
      </form>
    </div>
  );
}
