import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
// import App from './App.tsx'
import 'bootstrap/dist/css/bootstrap.min.css';
// import OrderForm from './Order.tsx';
import MultiOrderForm from './InvidialOrder.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MultiOrderForm />
  </StrictMode>,
)
