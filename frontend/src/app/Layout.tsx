import { NavLink, Outlet } from 'react-router-dom'

const navigation = [
  { to: '/contas', label: 'Contas' },
  { to: '/transferencias/nova', label: 'Nova transferência' },
  { to: '/transferencias', label: 'Transferências', end: true },
]

export function Layout() {
  return (
    <>
      <header className="layout__header">
        <div className="layout__header-inner">
          <span className="layout__brand">Brasil Terrenos</span>
          <nav className="layout__nav" aria-label="Navegação principal">
            {navigation.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className="layout__link"
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>
      <main className="layout__main">
        <Outlet />
      </main>
    </>
  )
}
