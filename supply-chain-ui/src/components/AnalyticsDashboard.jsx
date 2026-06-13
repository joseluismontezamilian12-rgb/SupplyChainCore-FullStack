import React, { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { DollarSign, Activity, AlertTriangle, RefreshCw } from 'lucide-react';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8'];

// 🎨 Definición de Diccionario de Estilos Nativos (Adiós dependencias)
const styles = {
  container: { backgroundColor: '#111827', color: '#f3f4f6', padding: '24px', borderRadius: '12px', marginTop: '40px', fontFamily: 'Arial, sans-serif', boxShadow: '0 10px 25px rgba(0,0,0,0.5)' },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '1px solid #374151', paddingBottom: '16px' },
  btnRefresh: { backgroundColor: '#1f2937', color: '#60a5fa', border: '1px solid #4b5563', padding: '8px 16px', borderRadius: '8px', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px', fontWeight: 'bold' },
  gridKpis: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '20px', marginTop: '24px' },
  card: { backgroundColor: '#1f2937', padding: '20px', borderRadius: '10px', border: '1px solid #1f2937', display: 'flex', justifyContent: 'space-between', alignItems: 'center' },
  iconBox: { padding: '12px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center' },
  gridCharts: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))', gap: '24px', marginTop: '32px' },
  chartCard: { backgroundColor: '#1f2937', padding: '20px', borderRadius: '10px', border: '1px solid #374151' }
};

export default function AnalyticsDashboard() {
  const [kpis, setKpis] = useState({ valorTotalInventario: 0, totalMovimientosMes: 0, tasaMermaGlobal: 0 });
  const [ocupacion, setOcupacion] = useState([]);
  const [tendencia, setTendencia] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchData = async () => {
    setLoading(true);
    try {
      const baseUrl = 'https://localhost:7047/api/analytics';
      const [resKpis, resOcupacion, resTendencia] = await Promise.all([
        fetch(`${baseUrl}/kpis`),
        fetch(`${baseUrl}/ocupacion`),
        fetch(`${baseUrl}/tendencia`)
      ]);

      if (resKpis.ok) setKpis(await resKpis.json());
      if (resOcupacion.ok) setOcupacion(await resOcupacion.json());
      if (resTendencia.ok) setTendencia(await resTendencia.json());
    } catch (error) {
      console.error("Error devorando datos analíticos:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '200px', color: '#60a5fa', fontFamily: 'Arial' }}>
        <RefreshCw className="animate-spin" style={{ marginRight: '10px' }} /> 
        <span>Procesando KPIs de Inteligencia de Negocio...</span>
      </div>
    );
  }

  return (
    <div style={styles.container}>
      
      {/* HEADER */}
      <div style={styles.header}>
        <div>
          <h2 style={{ margin: 0, fontSize: '22px', fontWeight: 'bold', color: '#fff' }}>📊 Enterprise Analytics Dashboard</h2>
          <p style={{ margin: '4px 0 0 0', fontSize: '13px', color: '#9ca3af' }}>Plataforma de Control Estratégico y KPIs Logísticos</p>
        </div>
        <button onClick={fetchData} style={styles.btnRefresh}>
          <RefreshCw size={16} /> Actualizar
        </button>
      </div>

      {/* 🟢 SECCIÓN 1: TARJETAS KPI */}
      <div style={styles.gridKpis}>
        {/* Tarjeta 1 */}
        <div style={styles.card}>
          <div>
            <span style={{ fontSize: '11px', color: '#9ca3af', textTransform: 'uppercase', fontWeight: 'bold' }}>Valor Neto del Inventario</span>
            <h3 style={{ margin: '6px 0 0 0', fontSize: '24px', color: '#34d399', fontWeight: 'bold' }}>S/ {kpis.valorTotalInventario.toLocaleString()}</h3>
          </div>
          <div style={{ ...styles.iconBox, backgroundColor: 'rgba(52,211,153,0.1)', color: '#34d399' }}><DollarSign size={24} /></div>
        </div>

        {/* Tarjeta 2 */}
        <div style={styles.card}>
          <div>
            <span style={{ fontSize: '11px', color: '#9ca3af', textTransform: 'uppercase', fontWeight: 'bold' }}>Volumen Operativo (Mes)</span>
            <h3 style={{ margin: '6px 0 0 0', fontSize: '24px', color: '#60a5fa', fontWeight: 'bold' }}>{kpis.totalMovimientosMes} Transacciones</h3>
          </div>
          <div style={{ ...styles.iconBox, backgroundColor: 'rgba(96,165,250,0.1)', color: '#60a5fa' }}><Activity size={24} /></div>
        </div>

        {/* Tarjeta 3 */}
        <div style={styles.card}>
          <div>
            <span style={{ fontSize: '11px', color: '#9ca3af', textTransform: 'uppercase', fontWeight: 'bold' }}>Tasa de Merma Global</span>
            <h3 style={{ margin: '6px 0 0 0', fontSize: '24px', color: '#fbbf24', fontWeight: 'bold' }}>{kpis.tasaMermaGlobal}%</h3>
          </div>
          <div style={{ ...styles.iconBox, backgroundColor: 'rgba(251,191,36,0.1)', color: '#fbbf24' }}><AlertTriangle size={24} /></div>
        </div>
      </div>

      {/* 🔵 SECCIÓN 2: CUADRÍCULA DE GRÁFICOS */}
      <div style={styles.gridCharts}>
        
        {/* Gráfico 1: Barras */}
        <div style={styles.chartCard}>
          <h4 style={{ margin: '0 0 16px 0', fontSize: '16px', color: '#fff' }}>📈 Tendencia Logística de Movimientos</h4>
          <div style={{ height: '280px', width: '100%' }}>
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={tendencia} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis dataKey="mesNombre" stroke="#9ca3af" fontSize={11} />
                <YAxis stroke="#9ca3af" fontSize={11} />
                <Tooltip contentStyle={{ backgroundColor: '#1f2937', borderColor: '#374151', color: '#fff' }} />
                <Legend />
                <Bar dataKey="totalIngresos" name="Ingresos (+)" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                <Bar dataKey="totalSalidas" name="Salidas/Mermas (-)" fill="#ef4444" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Gráfico 2: Dona */}
        <div style={styles.chartCard}>
          <h4 style={{ margin: '0 0 16px 0', fontSize: '16px', color: '#fff' }}>🍩 Distribución de Ocupación por Sede</h4>
          <div style={{ height: '280px', width: '100%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={ocupacion}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={85}
                  paddingAngle={5}
                  dataKey="totalUnidadesStock"
                  nameKey="almacenNombre"
                  label={({ almacenNombre, totalUnidadesStock }) => `${almacenNombre}: ${totalUnidadesStock} u`}
                >
                  {ocupacion.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ backgroundColor: '#1f2937', borderColor: '#374151', color: '#fff' }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>

      </div>

    </div>
  );
}