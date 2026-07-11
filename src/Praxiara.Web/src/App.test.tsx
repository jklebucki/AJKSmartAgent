import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { App } from './App'
import './shared/i18n/i18n'

describe('App', () => {
  it('renders the protected browser workspace shell', () => {
    render(<App />)

    expect(screen.getByRole('heading', { name: 'Praxiara' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Przeglądarka' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Zatwierdzenie' })).toBeInTheDocument()
  })
})
