import { useState } from 'react'
import reactLogo from '../assets/react.svg'
import viteLogo from '/electron-vite.animate.svg'
import '../App.css'
import BarGraph from './BarGraph'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <div>
        
      </div>
      <h1>WattWise Smart-Meter</h1>
      
      <p className="read-the-docs">
        <BarGraph/>
      </p>
    </>
  )
}

export default App
