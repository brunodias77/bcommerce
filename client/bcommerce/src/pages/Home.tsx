import React from 'react'
import Hero from '../components/Hero'
import Features from '../components/Features'
import Footer from '../components/Footer'
import NewLatter from '../components/NewLatter'
import PopularProducts from '../components/PopularProducts'
import Banner from '../components/Banner'
import About from '../components/About'
import Blog from '../components/Blog'
import NewArrivals from '../components/NewArrivals'

const Home: React.FC = () => {
    return (
        <>
            <Hero />
            <Features />
            <NewArrivals />
            <PopularProducts />
            <Banner />
            <About />
            <Blog />
            <NewLatter />
        </>
    )
}

export default Home
