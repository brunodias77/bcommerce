import React from 'react';
import Header from './components/Header';
import Home from './pages/Home';
import Collection from './pages/Collection';
import Blog from './pages/Blog';
import Cart from './pages/Cart';
import Product from './pages/Product';
import Footer from './components/Footer';
import PlaceOrder from './pages/PlaceOrder';
import { ToastContainer } from 'react-toastify';
import { Route, Routes } from 'react-router-dom';

const App: React.FC = () => {
  return (
    <main className="flex flex-col min-h-screen text-tertiary">
      <ToastContainer aria-label={undefined} />
      <Header />
      <div className="flex-1">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/collection" element={<Collection />} />
          <Route path="/blog" element={<Blog />} />
          <Route path='/product/:productId' element={<Product />} />
          <Route path="/cart" element={<Cart />} />
          <Route path="/place-order" element={<PlaceOrder />} />
        </Routes>
      </div>
      <Footer />
    </main>
    // <main className='overflow-hidden text-tertiary'>
    //   <ToastContainer aria-label={undefined} />
    //   <Header />
    //   <Routes>
    //     <Route path="/" element={<Home />} />
    //     <Route path="/collection" element={<Collection />} />
    //     <Route path="/blog" element={<Blog />} />
    //     <Route path='/product/:productId' element={<Product />} />
    //     <Route path="/cart" element={<Cart />} />
    //   </Routes>
    //   <Footer />
    // </main>
  );
};

export default App;
