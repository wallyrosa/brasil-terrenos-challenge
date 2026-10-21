import { Navigate, Route, Routes } from 'react-router-dom'
import { AccountDetailPage } from '@/features/accounts/pages/AccountDetailPage'
import { AccountsPage } from '@/features/accounts/pages/AccountsPage'
import { NewTransferPage } from '@/features/transfers/pages/NewTransferPage'
import { TransfersPage } from '@/features/transfers/pages/TransfersPage'
import { EmptyState } from '@/shared/ui'
import { Layout } from './Layout'
import { AppProviders } from './providers'

export function App() {
  return (
    <AppProviders>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<Navigate to="/contas" replace />} />
          <Route path="contas" element={<AccountsPage />} />
          <Route path="contas/:id" element={<AccountDetailPage />} />
          <Route path="transferencias" element={<TransfersPage />} />
          <Route path="transferencias/nova" element={<NewTransferPage />} />
          <Route path="*" element={<EmptyState title="Página não encontrada" />} />
        </Route>
      </Routes>
    </AppProviders>
  )
}
