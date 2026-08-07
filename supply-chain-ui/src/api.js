// Punto único de contacto con la API. La URL sale de una variable de entorno de
// Vite para que el mismo build sirva en local y en un despliegue real; el
// fallback es el puerto por defecto del proyecto .NET en desarrollo.
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7047';

const CLAVE_SESION = 'supplychaincore.sesion';

export function leerSesion() {
  try {
    const crudo = localStorage.getItem(CLAVE_SESION);
    if (!crudo) return null;

    const sesion = JSON.parse(crudo);

    // Un token vencido es igual de inútil que no tener token: se descarta aquí
    // para no mostrar una pantalla autenticada que fallaría en la primera llamada.
    if (!sesion?.token || new Date(sesion.expiraUtc) <= new Date()) {
      localStorage.removeItem(CLAVE_SESION);
      return null;
    }

    return sesion;
  } catch {
    localStorage.removeItem(CLAVE_SESION);
    return null;
  }
}

export function guardarSesion(sesion) {
  localStorage.setItem(CLAVE_SESION, JSON.stringify(sesion));
}

export function cerrarSesion() {
  localStorage.removeItem(CLAVE_SESION);
}

export async function login(email, password) {
  const respuesta = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  if (!respuesta.ok) {
    const data = await respuesta.json().catch(() => ({}));
    throw new Error(data.error ?? 'No se pudo iniciar sesión.');
  }

  const sesion = await respuesta.json();
  guardarSesion(sesion);
  return sesion;
}

/**
 * fetch con el token adjunto. Ante un 401 o 403 limpia la sesión y avisa, para
 * que la UI vuelva al login en vez de quedarse mostrando datos vacíos.
 */
export async function apiFetch(ruta, opciones = {}) {
  const sesion = leerSesion();

  const respuesta = await fetch(`${API_BASE_URL}${ruta}`, {
    ...opciones,
    headers: {
      'Content-Type': 'application/json',
      ...(sesion ? { Authorization: `Bearer ${sesion.token}` } : {}),
      ...opciones.headers
    }
  });

  if (respuesta.status === 401) {
    cerrarSesion();
    window.dispatchEvent(new Event('sesion-expirada'));
    throw new Error('La sesión expiró. Vuelve a iniciar sesión.');
  }

  if (respuesta.status === 403) {
    throw new Error('Tu rol no tiene permiso para esta operación.');
  }

  return respuesta;
}
