import React from 'react'
import cards from "../assets/cards.png"
import logo from "../assets/logo/logo.svg"
const Footer: React.FC = () => {
    return (
        <footer>
            <div className="max-padd-container flex items-start justify-between flex-wrap gap-12 mt-12">
                {/* logo - Left side */}
                <div className="flex flex-col max-w-sm gap-y-5">
                    <div className="max-w-[60px]">
                        <img src={logo} alt="logo da empresa" />
                    </div>
                    <p>
                        Lorem ipsum dolor sit amet consectetur adipisicing elit. Voluptas
                        earum reprehenderit possimus!
                    </p>
                    <img src={cards} alt="" height={33} width={144} className="mt-5" />
                </div>
                <div className="flex items-center justify-start gap-7 xl:gap-x-36 flex-wrap">
                    <ul>
                        <h4 className="h4 mb-3">Customer Service</h4>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Help center
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Payment methods
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Contact
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Shipping status
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Complaints
                            </a>
                        </li>
                    </ul>
                    <ul>
                        <h4 className="h4 mb-3">Legal</h4>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Privacy Policy
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Cookie settings
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Terms & conditions
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Cancelation
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Imprint
                            </a>
                        </li>
                    </ul>
                    <ul>
                        <h4 className="h4 mb-3">Others</h4>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Our teams
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Sustainability
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Press
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Jobs
                            </a>
                        </li>
                        <li className="my-2">
                            <a href="" className="text-gray-30 text-[14px] font-[400] ">
                                Newsletter
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            {/* copyrights */}
            <p className="text-center bg-primary text-gray-500  text-[14px] font-[500] py-2 px-8 rounded flexBetween mt-6">
                <span>2025 @bdias.dev</span>
                <span> All rights reserved</span>
            </p>
        </footer>
    )
}

export default Footer
